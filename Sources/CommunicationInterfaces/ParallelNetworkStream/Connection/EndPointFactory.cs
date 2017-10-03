using System.Net;

namespace NetworkOperator.CommunicationInterfaces.Connection
{
    class EndPointFactory
    {
        public int Port { get; private set; }
        public IPEndPoint BroadcastSend => Create(IPAddress.Broadcast);
        public IPEndPoint BrodcastReceive => new IPEndPoint(IPAddress.Any, 0);

        public EndPointFactory(int port)
        {
            Port = port;
        }
        public IPEndPoint Create(byte[] ip) => Create(new IPAddress(ip));
        public IPEndPoint Create(IPAddress ip) => new IPEndPoint(ip, Port);
    }
}
