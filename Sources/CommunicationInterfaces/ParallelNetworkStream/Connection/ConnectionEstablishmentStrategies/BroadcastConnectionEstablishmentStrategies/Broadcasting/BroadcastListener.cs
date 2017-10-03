using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies.Broadcasting
{
    public class BroadcastListener
    {
        private Action<IPEndPoint, byte[]> onDataHeard;
        public event Action<IPEndPoint, byte[]> OnDataReceived
        {
            add
            {
                onDataHeard += value;
            }
            remove
            {
                onDataHeard -= value;
            }
        }

        public bool IsListenning { get; private set; } = false;

        private BroadcastClient client;
        public BroadcastListener(BroadcastClient client)
        {
            this.client = client;
        }
        public void Listen() => Listen(true);
        public void Listen(bool sempiternally)
        {
            IsListenning = true;
            var localAddresses = new List<IPAddress>();
            foreach (var ip in
                from address in Dns.GetHostAddresses(Dns.GetHostName())
                where address.AddressFamily == AddressFamily.InterNetwork
                select address)
            {
                localAddresses.Add(ip);
                client.BindIfNecessary(ip);
            }
            byte[] data = null;
            IPEndPoint sender;
            while (IsListenning)
            {
                try
                {
                    sender = client.Receive(out data);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut && sempiternally)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (sender == null)
                {
                    break;
                }
                else if (!localAddresses.Contains(sender.Address))
                {
                    onDataHeard(sender, data);
                }
            }
            IsListenning = false;
        }
        public void StopListenning()
        {
            IsListenning = false;
        }
    }
}
