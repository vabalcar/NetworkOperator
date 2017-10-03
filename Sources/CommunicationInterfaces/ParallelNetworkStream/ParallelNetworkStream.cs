using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming;
using NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies;
using NetworkOperator.CommunicationInterfaces.VariableWatching;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Immutable;

namespace NetworkOperator.CommunicationInterfaces
{
    public class ParallelNetworkStream : IDisposable
    {
        public const int UDP_PORT = 6000;

        public class ClientRequestor
        {
            private ParallelNetworkStream parent;

            private const short EMPTY_BROADCASTED_TYPE = -1;

            private WatchedVariable<short> broadcastedType;
            private WatchedVariable<bool> broadcastEverything;
            private EndPointFactory remoteEndPointFactory;
            public bool BroadcastEverything
            {
                get
                {
                    return broadcastEverything.Value;
                }
                set
                {
                    SendBroadcastEverythingRequest(value);
                }
            }
            private Action<string> onClientConnected;
            public event Action<string> OnClientConnected
            {
                add
                {
                    onClientConnected += value;
                }
                remove
                {
                    onClientConnected -= value;
                }
            }
            private Action<string> onClientDisconnected;
            public event Action<string> OnClientDisconnected
            {
                add
                {
                    onClientDisconnected += value;
                }
                remove
                {
                    onClientDisconnected -= value;
                }
            }
            public ImmutableDictionary<string, IPEndPoint> ConnectableClients { get; private set; } 
                = ImmutableDictionary.Create<string, IPEndPoint>();

            public Reliability TransferReliability { get; private set; }

            public ClientRequestor(ParallelNetworkStream parent)
            {
                this.parent = parent;
                broadcastedType = new WatchedVariable<short>(parent.watcher) { Value = EMPTY_BROADCASTED_TYPE };
                broadcastEverything = new WatchedVariable<bool>(parent.watcher) { Value = false };
                remoteEndPointFactory = parent.udpEndPointFactory;
            }

            internal void ProcessInternalMessage(InternalMessage message)
            {
                switch (message.MessageType)
                {
                    case InternalMessageType.ClientList:
                        StringReader reader = new StringReader(message.Content);
                        string client;
                        while ((client = reader.ReadLine()) != null)
                        {
                            AddConnectableClient(client);
                        }
                        break;
                    case InternalMessageType.ClientConnected:
                        AddConnectableClient(message.Content, onClientConnected);
                        break;
                    case InternalMessageType.ClientDisconnected:
                        ConnectableClients = ConnectableClients.Remove(message.Content);
                        onClientDisconnected?.Invoke(message.Content);
                        parent.onRemoteHostDisconnected?.Invoke();
                        break;
                    case InternalMessageType.RegistrationRequest:
                        parent.remoteHostName.Value = message.Sender;
                        break;
                    case InternalMessageType.ConnectionRequest:
                        parent.onRemoteJoinedHostName.Value = message.Content;
                        break;
                    case InternalMessageType.BroacastRequest:
                        short setBroadcastedType = short.Parse(message.Content);
                        switch (setBroadcastedType)
                        {
                            case (short)BroadcastSettingCodes.Cancel:
                                broadcastedType.Value = EMPTY_BROADCASTED_TYPE;
                                broadcastEverything.Value = false;
                                break;
                            case (short)BroadcastSettingCodes.BroadcastEverythingTrue:
                                broadcastEverything.Value = true;
                                break;
                            case (short)BroadcastSettingCodes.BroadcastEverythingFalse:
                                broadcastEverything.Value = false;
                                break;
                            default:
                                broadcastedType.Value = setBroadcastedType;
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            private void AddConnectableClient(string clientDescription)
                => AddConnectableClient(clientDescription, null);
            private void AddConnectableClient(string clientDescription, Action<string> onSuccessAction)
            {
                string[] tokens = clientDescription.Split('@');
                if (tokens.Length != 2
                    || !IPAddress.TryParse(tokens[1], out IPAddress ipAddress))
                {
                    throw new FormatException("Invalid format of connectable client.");
                }
                ConnectableClients = ConnectableClients.Add(tokens[0], remoteEndPointFactory.Create(ipAddress));
                onSuccessAction?.Invoke(tokens[0]);
            }
            internal void MakeIntroduction() => MakeIntroduction(true);
            internal Action MakeIntroduction(bool immediateIntoduction)
            {
                if (parent.localHostName.Length == 0)
                {
                    throw new InvalidOperationException($"Name cannot be updated before {nameof(ParallelNetworkStream)} is opened.");
                }
                string originalRemoteHostName = parent.remoteHostName.Value;
                Action introduction = () => parent.Write(new InternalMessage
                {
                    MessageType = InternalMessageType.RegistrationRequest,
                    Sender = parent.localHostName
                });
                if (immediateIntoduction)
                {
                    introduction();
                }
                parent.watcher.WaitForChange(parent.remoteHostName, originalRemoteHostName, parent.Dispose);
                return immediateIntoduction? null : introduction;
            }

            public void ConnectWith(string hostName, Reliability requestedTransferReliability)
            {
                parent.ParallelStream.SetOnWriteToNull();
                switch (requestedTransferReliability)
                {
                    case Reliability.Reliable:
                        string originalJoinedHostOnRemote = parent.onRemoteJoinedHostName.Value;
                        parent.Write(new InternalMessage
                        {
                            MessageType = InternalMessageType.ConnectionRequest,
                            Sender = parent.LocalHostName,
                            Content = hostName
                        });
                        parent.watcher.WaitForChange(parent.onRemoteJoinedHostName, originalJoinedHostOnRemote, parent.Dispose);
                        break;
                    case Reliability.Unreliable:
                        if (!parent.EnableUnreliableTransfers)
                        {
                            throw new InvalidOperationException($"Unrelieble transfers have been disabled!");
                        }
                        parent.onRemoteJoinedHostName.Value = hostName;
                        parent.ParallelStream.OnWrite += parent.WriteDataUreliable;
                        break;
                    default:
                        break;
                }
                TransferReliability = requestedTransferReliability;
            }

            public void CancelBroadcasting()
            {
                short originalBroadcastedType = broadcastedType.Value;
                bool originalBroadcastEverythingStatus = broadcastEverything.Value;
                short requiredBroadcastedType = (short)BroadcastSettingCodes.Cancel;
                if (originalBroadcastedType == requiredBroadcastedType)
                {
                    return;
                }
                parent.Write(new InternalMessage
                {
                    MessageType = InternalMessageType.BroacastRequest,
                    Sender = parent.LocalHostName,
                    Content = requiredBroadcastedType.ToString()
                });
                parent.watcher.WaitForChange(broadcastedType, originalBroadcastedType, parent.Dispose);
                parent.watcher.WaitForChange(broadcastEverything, originalBroadcastEverythingStatus, parent.Dispose);
                parent.ParallelStream.CancelBroadcasting();
            }
            private void SendBroadcastEverythingRequest(bool value)
            {
                bool originalBroadcastEverythingStatus = broadcastEverything.Value;
                short requiredBroadcastedType = value == true ?
                    (short)BroadcastSettingCodes.BroadcastEverythingTrue : (short)BroadcastSettingCodes.BroadcastEverythingFalse;
                if (originalBroadcastEverythingStatus == value)
                {
                    return;
                }
                parent.Write(new InternalMessage
                {
                    MessageType = InternalMessageType.BroacastRequest,
                    Sender = parent.LocalHostName,
                    Content = requiredBroadcastedType.ToString()
                });
                parent.watcher.WaitForChange(broadcastEverything, originalBroadcastEverythingStatus, parent.Dispose);
                parent.ParallelStream.BroadcastEverything = value;
            }
            public void SetBroadcastedType<T>()
            {
                short originalBroadcastedType = broadcastedType.Value;
                short requiredBroadcastedType = parent.ParallelStream.GetTypeId<T>();
                if (originalBroadcastedType == requiredBroadcastedType)
                {
                    return;
                }
                parent.Write(new InternalMessage
                {
                    MessageType = InternalMessageType.BroacastRequest,
                    Sender = parent.LocalHostName,
                    Content = requiredBroadcastedType.ToString()
                });
                parent.watcher.WaitForChange(broadcastedType, originalBroadcastedType, parent.Dispose);
                parent.ParallelStream.SetBroadcastedType(requiredBroadcastedType);
            }
        }
        public ClientRequestor ClientRequests { get; private set; }
        public class ServerOperator
        {
            private ParallelNetworkStream parent;

            public bool BroadcastEverything
            {
                get
                {
                    return parent.ParallelStream.BroadcastEverything;
                }
                set
                {
                    parent.ParallelStream.BroadcastEverything = value;
                }
            }
            public ServerOperator(ParallelNetworkStream parent)
            {
                this.parent = parent;
            }

            public bool IsJoined => parent.ParallelStream.JoinedStream != null;
            public void Join(ParallelNetworkStream targetStream) => parent.ParallelStream.Join(targetStream.ParallelStream);
            public void Disjoin() => parent.ParallelStream.Disjoin();

            public void SetBroadcastedType<T>() => parent.ParallelStream.SetBroadcastedType<T>();
            public void SetBroadcastedType(short typeId) => parent.ParallelStream.SetBroadcastedType(typeId);
            public void CancelBroadcasting() => parent.ParallelStream.CancelBroadcasting();
        }
        public ServerOperator ServerOperations { get; private set; }
        private class EnumerableBroadcastList : IEnumerable<ParallelMultiTypeStream>
        {
            public IEnumerable<ParallelNetworkStream> BroadcastList { get; set; }
            public IEnumerator<ParallelMultiTypeStream> GetEnumerator()
            {
                foreach (var stream in BroadcastList)
                {
                    yield return stream.ParallelStream;
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public IEnumerable<ParallelNetworkStream> BroadcastList
        {
            set
            {
                ParallelStream.BroadcastList = new EnumerableBroadcastList() { BroadcastList = value };
            }
        }
        public bool BroadcastEverything
        {
            get
            {
                return ParallelStream.BroadcastEverything;
            }
            set
            {
                ParallelStream.BroadcastEverything = value;
            }
        }

        private Action onInternalErrorDetected;
        public event Action OnInternalErrorDetected
        {
            add
            {
                onInternalErrorDetected += value;
            }
            remove
            {
                onInternalErrorDetected += value;
            }
        }
        private Action onConnectionInterrupted;
        public event Action OnConnectionInterrupted
        {
            add
            {
                onConnectionInterrupted += value;
            }
            remove
            {
                onConnectionInterrupted -= value;
            }
        }
        private Action onRemoteHostDisconnected;
        public event Action OnRemoteHostDisconnected
        {
            add
            {
                onRemoteHostDisconnected += value;
            }
            remove
            {
                onRemoteHostDisconnected -= value;
            }
        }
        public event Action OnConnectionEnded
        {
            add
            {
                onInternalErrorDetected += value;
                onConnectionInterrupted += value;
                onRemoteHostDisconnected += value;
            }
            remove
            {
                onInternalErrorDetected -= value;
                onConnectionInterrupted -= value;
                onRemoteHostDisconnected -= value;
            }
        }

        public bool IsOpened { get; private set; } = false;
        public bool IsOpening
        {
            get
            {
                if (connector == null)
                {
                    return false;
                }
                return connector.IsEstablishingConnection;
            }
        }
        
        private bool nameSet = false;
        private string localHostName = Dns.GetHostName();
        public string LocalHostName
        {
            get
            {
                return localHostName;
            }
            set
            {
                if (nameSet)
                {
                    throw new InvalidOperationException($"{nameof(LocalHostName)} can be set only before {nameof(ParallelNetworkStream)} is opened.");
                }
                if (value.Length > 0 && value.IndexOf('\n') == -1)
                {
                    localHostName = value;
                }
                else
                {
                    throw new ArgumentException($"Invalid {nameof(localHostName)}");
                }
            }
        }
        private WatchedVariable<string> remoteHostName;
        public string RemoteHostName
        {
            get
            {
                return remoteHostName.Value;
            }
        }
        private WatchedVariable<string> onRemoteJoinedHostName;

        public IPAddress LocalHostIPAddress { get; private set; }
        public IPAddress RemoteHostIPAddress { get; private set; }
        public int LocalPort
        {
            get
            {
                if (connector == null)
                {
                    return ((IPEndPoint)socket.LocalEndPoint).Port;
                }
                else
                {
                    return connector.LocalPort;
                }
            }
        }
        public int RemotePort
        {
            get
            {
                if (connector == null)
                {
                    return ((IPEndPoint)socket.RemoteEndPoint).Port;
                }
                else
                {
                    return connector.RemotePort;
                }
            }
        }

        private bool enableUnreliableConnections = true;
        internal bool EnableUnreliableTransfers
        {
            get => enableUnreliableConnections;
            set
            {
                if (IsOpening || IsOpened)
                {
                    throw new InvalidOperationException($"{nameof(EnableUnreliableTransfers)} can be set only before {nameof(ParallelNetworkStream)} is opened.");
                }
                enableUnreliableConnections = value;
            }
        }

        private NetworkStream networkStream;
        private Connector connector;
        private UdpClient udpClient = new UdpClient();
        private EndPointFactory udpEndPointFactory = new EndPointFactory(UDP_PORT);
        protected ParallelMultiTypeStream ParallelStream { get; private set; }
        private Socket socket;

        private int timeout = 30;//seconds
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
                watcher.Timeout = value;
            }
        }
        private const int timeoutTickLength = 1000;//ms

        private VariableWatcher watcher;

        private object disposeLock = new object();
        private bool disposed = false;

        public ParallelNetworkStream(Socket clientSocket) 
            : this(new ParallelMultiTypeStream(), clientSocket)
        {
        }
        protected ParallelNetworkStream(ParallelMultiTypeStream parallelStream, Socket clientSocket)
        {
            this.ParallelStream = parallelStream;
            socket = clientSocket;
            Init();
        }
        public ParallelNetworkStream(IConnectionEstablishmentStrategy connectionEstablishmentStrategy)
            : this(new ParallelMultiTypeStream(), connectionEstablishmentStrategy)
        {
        }
        protected ParallelNetworkStream(ParallelMultiTypeStream parallelStream, 
            IConnectionEstablishmentStrategy connectionEstablishmentStrategy)
        {
            this.ParallelStream = parallelStream;
            connector = new Connector(connectionEstablishmentStrategy);
            Init();
        }
        public ParallelNetworkStream(int localPort, TcpRole role) 
            : this(new ParallelMultiTypeStream(), localPort, role)
        {
        }
        protected ParallelNetworkStream(ParallelMultiTypeStream parallelStream, int localPort, TcpRole role)
        {
            this.ParallelStream = parallelStream;
            connector = new Connector(new AutoSearchStrategy(localPort) { Role = role });
            Init();
        }
        private void Init()
        {
            watcher = new VariableWatcher(timeoutTickLength) { Timeout = this.Timeout };
            ClientRequests = new ClientRequestor(this);
            ServerOperations = new ServerOperator(this);
            AddReceivedDataHandler<InternalMessage>(ClientRequests.ProcessInternalMessage);            
            remoteHostName = watcher.Factory.Create(string.Empty);
            onRemoteJoinedHostName = watcher.Factory.Create(string.Empty);
        }

        public void AddReceivedDataHandler<T>(Action<T> handler) => ParallelStream.AddReceivedDataHandler(handler);
        public void RemoveReceivedDataHandler<T>(Action<T> handler) => ParallelStream.RemoveReceivedDataHandler(handler);

        public void Open() => Open(true);
        internal Action Open(bool immediateIntroduction)
        {
            if (IsOpening)
            {
                if (!disposed)
                {
                    throw new InvalidOperationException($"{nameof(ParallelNetworkStream)} is already opening.");
                }
            }
            if (IsOpened)
            {
                if (!disposed)
                {
                    throw new InvalidOperationException($"{nameof(ParallelNetworkStream)} has been already opened.");
                }
            }
            if (socket == null)
            {
                socket = connector.EstablishConnection();
                if (socket == null)
                {
                    return null;
                }
            }
            LocalHostIPAddress = ((IPEndPoint)socket.LocalEndPoint).Address;
            RemoteHostIPAddress = ((IPEndPoint)socket.RemoteEndPoint).Address;
            networkStream = new NetworkStream(socket)
            {
                ReadTimeout = timeoutTickLength
            };
            ParallelStream.Open(networkStream);
            if (EnableUnreliableTransfers)
            {
                IPAddress addressToBind;
                if (IPAddress.IsLoopback(LocalHostIPAddress))
                {
                    addressToBind = connector.ConnectionEstablishmentStrategy.ServerIPAddress;
                }
                else
                {
                    addressToBind = LocalHostIPAddress;
                }
                udpClient.Client.Bind(udpEndPointFactory.Create(addressToBind));
            }
            StartReading();
            IsOpened = true;
            return ClientRequests.MakeIntroduction(immediateIntroduction);
        }
        public void CancelOpenning()
        {
            if (IsOpening && connector != null)
            {
                connector.CancelConnectionEstablishment();
            }
        }
        private void StartReading()
        {
            if (EnableUnreliableTransfers)
            {
                new Thread(() =>
                {
                    IPEndPoint ipEndPoint = null;
                    while (true)
                    {
                        try
                        {
                            ParallelStream.ProcessReadData(udpClient.Receive(ref ipEndPoint));
                        }
                        catch (SocketException)
                        {
                            break;
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }
                    }
                }).Start();
            }
            new Thread(() =>
            {
                while (true)
                {
                    BeginningOfReceiving:;
                    try
                    {
                        ParallelStream.ReadNextData();
                    }
                    catch (IOException e)
                    {
                        if (e.InnerException is SocketException se)
                        {
                            switch (se.SocketErrorCode)
                            {
                                //standard time-out reaction
                                case SocketError.TimedOut:
                                    if (!disposed)
                                    {
                                        goto BeginningOfReceiving;
                                    }
                                    else
                                    {
                                        goto EndOfReceiving;
                                    }
                                //graceful shutdown in progress:
                                case SocketError.Disconnecting:
                                case SocketError.OperationAborted:
                                case SocketError.Shutdown:
                                    goto EndOfReceiving;
                                //local system or .NET errors:
                                case SocketError.Fault:
                                case SocketError.Interrupted:
                                case SocketError.AccessDenied:
                                case SocketError.TooManyOpenSockets:
                                case SocketError.SocketError:
                                case SocketError.ProcessLimit:
                                case SocketError.SystemNotReady:
                                case SocketError.NoBufferSpaceAvailable:
                                case SocketError.ConnectionAborted:
                                    if (onInternalErrorDetected != null)
                                    {
                                        Task.Factory.StartNew(onInternalErrorDetected);
                                    }
                                    goto default;
                                //network problems:
                                case SocketError.NetworkDown:
                                case SocketError.NetworkUnreachable:
                                case SocketError.HostUnreachable:
                                    if (onConnectionInterrupted != null)
                                    {
                                        Task.Factory.StartNew(onConnectionInterrupted);
                                    }
                                    goto default;
                                //remote host problems:
                                case SocketError.HostDown:
                                case SocketError.ConnectionRefused:
                                case SocketError.ConnectionReset:
                                    if (onRemoteHostDisconnected != null)
                                    {
                                        Task.Factory.StartNew(onRemoteHostDisconnected);
                                    }
                                    goto default;
                                default:
                                    Dispose();
                                    goto EndOfReceiving;
                            }
                        }
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        goto EndOfReceiving;
                    }
                }
                Dispose();
                EndOfReceiving:;
            }).Start();
        }

        private void WriteDataUreliable(byte[] data)
        {
            if (onRemoteJoinedHostName != null 
                && ClientRequests.ConnectableClients.TryGetValue(onRemoteJoinedHostName.Value, out IPEndPoint receiver))
            {
                udpClient.Send(data, data.Length, receiver);
            }
        }
        public void Write<T>(T data) => Write(data, ParallelStream.Write);
        public void Write(byte[] data) => Write(data, ParallelStream.Write);
        public void Write(string data) => Write(data, ParallelStream.Write);
        public void Write(InternalMessage message) => Write(message, ParallelStream.Write);
        protected void Write<T>(T data, Action<T> sendAction)
        {
            if (!IsOpened)
            {
                if (!disposed)
                {
                    throw new InvalidOperationException($"{nameof(ParallelNetworkStream)} must be opened before calling {nameof(Write)} method. Please call {nameof(Open)} method first.");
                }
                return;
            }
            try
            {
                sendAction(data);
            }
            catch (IOException)
            {
                //this situation will be solved on another thread
            }
            catch (ObjectDisposedException)
            {
                //writer has been disposed on another thread, nothing to do here
            }
        }

        public void Close() => Dispose();

        public void Dispose()
        {
            lock(disposeLock)
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
                IsOpened = false;
                if (networkStream != null)
                {
                    networkStream.Flush();
                    networkStream.Dispose();
                }
                if (connector != null)
                {
                    connector.Dispose();
                }
                udpClient.Close();
            }
        }

        public override string ToString() => $"{LocalHostName}@{LocalHostIPAddress}:{LocalPort} <-> {RemoteHostName}@{RemoteHostIPAddress}:{RemotePort}";
    }
    public enum BroadcastSettingCodes : short
    {
        Cancel = -2, BroadcastEverythingTrue = -3, BroadcastEverythingFalse = -4
    }
    public enum Reliability : byte
    {
        Reliable, Unreliable
    }
}
