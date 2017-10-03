using NetworkOperator.DataStructures.ThreadSafe;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace NetworkOperator.CommunicationInterfaces.Connection
{
    class Server : IDisposable
    {
        private static Server current;
        public static Server Current
        {
            get
            {
                if (current == null)
                {
                    current = new Server();
                }
                return current;
            }
        }
        private bool isAddressSet = false;
        public IPAddress Address { get; private set; }

        private ShareableImmutableDictionary<string, Client> clients = new ShareableImmutableDictionary<string, Client>();
        private SemaphoreSlim localClientConnectionSemaphore;

        private Server()
        {
        }

        public void Start(Socket serverSocket)
        {
            isAddressSet = false;
            localClientConnectionSemaphore = new SemaphoreSlim(0);
            while (true)
            {
                Socket clientSocket = null;
                try
                {
                    clientSocket = serverSocket.Accept();
                    Connector.SetupSocket(clientSocket);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        break;
                    }
                }
                new Thread(() => CompleteConnecting(clientSocket)).Start();
            }
        }
        private void CompleteConnecting(Socket socket)
        {
            Client client = new Client(socket);
            if (client.IsConnected)
            {
                throw new InvalidOperationException($"{nameof(Client)} {client} has been already fully connected.");
            }
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            if (!isAddressSet && !IPAddress.IsLoopback(((IPEndPoint)socket.RemoteEndPoint).Address))
            {
                Address = ((IPEndPoint)socket.LocalEndPoint).Address;
                isAddressSet = true;
                localClientConnectionSemaphore.Release();
            }
            else if (!isAddressSet)
            {
                localClientConnectionSemaphore.Wait();
            }
            client.OnClientRequestReceived += ProcessMessage;
            client.Connect();
            if (client.IsConnected)
            {
                client.Register(clients);
                sw.Stop();
                Console.WriteLine($"Client {clients[client.Name]} connected in {sw.ElapsedMilliseconds}ms");
            }
        }
        private void ProcessMessage(InternalMessage message)
        {
            switch (message.MessageType)
            {
                case InternalMessageType.RegistrationRequest:
                    break;
                case InternalMessageType.ConnectionRequest:
                    EstablishConnection(message.Sender, message.Content);
                    SendResponse(message);
                    break;
                case InternalMessageType.BroacastRequest:
                    if (clients.TryGetValue(message.Sender, out Client client))
                    {
                        short requiredBroadcastedType = short.Parse(message.Content);
                        switch (requiredBroadcastedType)
                        {
                            case (short)BroadcastSettingCodes.Cancel:
                                client.Operations.CancelBroadcasting();
                                break;
                            case (short)BroadcastSettingCodes.BroadcastEverythingTrue:
                                client.Operations.BroadcastEverything = true;
                                break;
                            case (short)BroadcastSettingCodes.BroadcastEverythingFalse:
                                client.Operations.BroadcastEverything = false;
                                break;
                            default:
                                client.Operations.SetBroadcastedType(requiredBroadcastedType);
                                break;
                        }
                        client.SendMessage(message);
                    }
                    break;
                default:
                    break;
            }
        }
        private void SendResponse(InternalMessage message)
        {
            if (clients.TryGetValue(message.Sender, out Client client))
            {
                client.SendMessage(message);
            }
        }
        private void EstablishConnection(string fromClient, string toClient)
        {
            if (clients.TryGetValue(fromClient, out Client client1)
                && clients.TryGetValue(toClient, out Client client2))
            {
                client1 += client2;
            }
        }
        public void Dispose()
        {
            foreach (var client in clients)
            {
                client.Dispose();
            }
        }
    }
}
