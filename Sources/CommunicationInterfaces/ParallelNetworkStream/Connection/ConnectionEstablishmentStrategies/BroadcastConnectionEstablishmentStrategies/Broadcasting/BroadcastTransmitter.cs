using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies.Broadcasting
{
    public class BroadcastTransmitter
    {
        public bool IsBroadcasting { get; private set; } = false;

        private BroadcastClient client;

        public BroadcastTransmitter(BroadcastClient client)
        {
            this.client = client;
        }
        public void BroadcastIPAddress() => Broadcast(null);
        public void Broadcast(byte[] data) => Broadcast(data, 100);
        public void Broadcast(byte[] data, int period) => Broadcast(data, period, -1);
        public void Broadcast(byte[] data, int period, int count)
        {
            IsBroadcasting = true;
            for (int i = 0; count < 0 ? true : i < count; i++)
            {
                foreach (var ip in
                    from address in Dns.GetHostAddresses(Dns.GetHostName())
                    where address.AddressFamily == AddressFamily.InterNetwork
                    select address)
                {
                    client.BindIfNecessary(ip);
                    if (data == null)
                    {
                        data = new byte[0];
                    }
                    client.Send(data);
                    if (!IsBroadcasting)
                    {
                        break;
                    }
                    if (period > 0)
                    {
                        Thread.Sleep(period);
                    }
                }
                if (!IsBroadcasting)
                {
                    break;
                }
            }
            IsBroadcasting = false;
        }
        public void StopBroacasting()
        {
            IsBroadcasting = false;
        }
    }
}
