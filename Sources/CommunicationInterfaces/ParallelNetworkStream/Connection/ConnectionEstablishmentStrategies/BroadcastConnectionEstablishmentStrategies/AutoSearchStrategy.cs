using System.Net;
using System.Threading.Tasks;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies
{
    public class AutoSearchStrategy : BroadcastConnectionEstablishmentStrategy
    {
        public AutoSearchStrategy(int localPort) : base(localPort)
        {
        }
        protected override void DoRunAsClient() => listener.Listen();
        protected override void DoRunAsServer() => Task.Factory.StartNew(broadcaster.BroadcastIPAddress);
        protected override void OnDataReceived(IPEndPoint sender, byte[] data)
        {
            ServerIPAddress = sender.Address;
            RemotePort = sender.Port;
            listener.StopListenning();
        }
    }
}
