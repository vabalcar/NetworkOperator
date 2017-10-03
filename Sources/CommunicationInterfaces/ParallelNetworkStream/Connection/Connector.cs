using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.Connection
{
    class Connector : IDisposable
    {
        public bool IsEstablishingConnection
        {
            get
            {
                return ConnectionEstablishmentStrategy.IsRunning || isAcceptingConnection || isConnecting;
            }
        }
        public int LocalPort => ConnectionEstablishmentStrategy.LocalPort;
        public int RemotePort => ConnectionEstablishmentStrategy.RemotePort;

        private bool isAcceptingConnection = false;
        private bool isConnecting = false;
        private EndPointFactory localEndPointFactory;
        private EndPointFactory remoteEndPointFactory;
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        internal IConnectionEstablishmentStrategy ConnectionEstablishmentStrategy { get; private set; }

        public Connector(IConnectionEstablishmentStrategy connectionEstablishmentStrategy)
        {
            this.ConnectionEstablishmentStrategy = connectionEstablishmentStrategy;
            localEndPointFactory = new EndPointFactory(connectionEstablishmentStrategy.LocalPort);
            serverSocket.Bind(localEndPointFactory.Create(IPAddress.Any));
            SetupSocket(serverSocket);
            serverSocket.Listen(10);
        }
        public static void SetupSocket(Socket socket)
        {
            //socket.NoDelay = true;
            //socket.SendBufferSize = 1024;
        }
        public Socket EstablishConnection()
        {
            ConnectionEstablishmentStrategy.Run();
            if (!ConnectionEstablishmentStrategy.IsRunning)
            {
                return null;
            }
            ConnectionEstablishmentStrategy.Stop();
            remoteEndPointFactory = new EndPointFactory(ConnectionEstablishmentStrategy.RemotePort);
            Socket clientSocket = null;
            if (ConnectionEstablishmentStrategy.IsLocalHostServer)
            {
                if (ConnectionEstablishmentStrategy.MultipleClients)
                {
                    ConnectionEstablishmentStrategy.Role = TcpRole.Server;
                    new Thread((ConnectionEstablishmentStrategy.Run)).Start();
                    new Thread(() => Server.Current.Start(serverSocket)).Start();
                    clientSocket = Connect(localEndPointFactory.Create(IPAddress.Loopback));
                }
                else
                {
                    clientSocket = AcceptConnection();
                }
            }
            else if (ConnectionEstablishmentStrategy.ServerIPAddress != null)
            {
                clientSocket = Connect(ConnectionEstablishmentStrategy.ServerIPAddress);
            }
            return clientSocket;
        }
        public void CancelConnectionEstablishment()
        {
            if (ConnectionEstablishmentStrategy.IsRunning)
            {
                ConnectionEstablishmentStrategy.Stop();
            }
            if (isAcceptingConnection)
            {
                CancelAcceptConnection();
            }
        }
        private Socket AcceptConnection()
        {
            Socket clientSocket = null;
            isAcceptingConnection = true;
            try
            {
                clientSocket = serverSocket.Accept();
                SetupSocket(clientSocket);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == (int)SocketError.Interrupted)
                {
                    isAcceptingConnection = false;
                    return null;
                }
            }
            finally
            {
                isAcceptingConnection = false;
            }
            return clientSocket;
        }
        private void CancelAcceptConnection()
        {
            if (isAcceptingConnection)
            {
                serverSocket.Close();
            }
        }
        private Socket Connect(IPAddress serverIPAddress) => Connect(remoteEndPointFactory.Create(serverIPAddress));
        private Socket Connect(EndPoint serverEndPoint)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupSocket(clientSocket);
            isConnecting = true;
            clientSocket.Connect(serverEndPoint);
            isConnecting = false;
            return clientSocket;
        }
        public void Dispose()
        {
            if (IsEstablishingConnection)
            {
                CancelConnectionEstablishment();
            }
            ConnectionEstablishmentStrategy.Dispose();
            serverSocket.Dispose();
            Server.Current.Dispose();
        }
    }
}
