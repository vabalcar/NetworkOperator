using NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies.Broadcasting;
using System.Net;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies
{
    public abstract class BroadcastConnectionEstablishmentStrategy : IConnectionEstablishmentStrategy
    {
        public int LocalPort { get; protected set; }
        public int RemotePort { get; protected set; }
        public TcpRole Role { get; set; } = TcpRole.Client;
        public bool IsLocalHostServer { get; protected set; }
        public IPAddress ServerIPAddress { get; protected set; }
        public bool MultipleClients { get; protected set; } = false;

        public bool IsRunning { get; protected set; } = false;

        protected BroadcastClient broadcastClient;
        protected BroadcastListener listener;
        protected BroadcastTransmitter broadcaster;

        public BroadcastConnectionEstablishmentStrategy(int localPort)
        {
            LocalPort = localPort;
            broadcastClient = new BroadcastClient(localPort);
            listener = new BroadcastListener(broadcastClient);
            listener.OnDataReceived += OnDataReceived;
            broadcaster = new BroadcastTransmitter(broadcastClient);
        }
        public void Run()
        {
            IsRunning = true;
            switch (Role)
            {
                case TcpRole.Client:
                    IsLocalHostServer = false;
                    DoRunAsClient();
                    break;
                case TcpRole.Server:
                    IsLocalHostServer = true;
                    DoRunAsServer();
                    break;
                default:
                    break;
            }
        }
        protected abstract void DoRunAsClient();
        protected abstract void DoRunAsServer();
        protected abstract void OnDataReceived(IPEndPoint sender, byte[] data);
        public virtual void Stop()
        {
            if (listener.IsListenning)
            {
                listener.StopListenning();
            }
            if (broadcaster.IsBroadcasting)
            {
                broadcaster.StopBroacasting();
            }
            IsRunning = false;
        }
        public void Dispose()
        {
            Stop();
            broadcastClient.Close();
        }
    }
}
