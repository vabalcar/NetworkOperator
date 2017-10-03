using NetworkOperator.Informants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies
{
    public class HighPerformanceServerStrategy : BroadcastConnectionEstablishmentStrategy
    {
        public new TcpRole Role
        {
            get
            {
                return base.Role;
            }
            private set
            {
                base.Role = value;
            }
        }
        private Dictionary<uint, IPEndPoint> communicants = new Dictionary<uint, IPEndPoint>();
        private EndPointFactory endPointFactory;
        public HighPerformanceServerStrategy(int localPort) : base(localPort)
        {
            Role = TcpRole.Client;
            MultipleClients = true;
            endPointFactory = new EndPointFactory(localPort);
            communicants.Add(ComputerInfo.Current.PerformanceIndex, endPointFactory.Create(IPAddress.Loopback));
        }

        protected override void DoRunAsClient()
        {
            new Thread(listener.Listen).Start();
            do
            {
                broadcaster.Broadcast(BitConverter.GetBytes(ComputerInfo.Current.PerformanceIndex), 50, 10);//takes 500ms
                if (!IsRunning)
                {
                    return;
                }
            }
            while (communicants.Count == 1);
            uint maxPerformanceIndex = communicants.Keys.Max();
            ServerIPAddress = communicants[maxPerformanceIndex].Address;
            if (IPAddress.IsLoopback(ServerIPAddress))
            {
                ServerIPAddress = GetLocalServerAddress();
                IsLocalHostServer = true;
            }
            else
            {
                RemotePort = communicants[maxPerformanceIndex].Port;
            }
        }
        private IPAddress GetLocalServerAddress()
        {
            byte[] netAddressBytes = new byte[3];
            foreach (var communicant in communicants)
            {
                if (!IPAddress.IsLoopback(communicant.Value.Address))
                {
                    byte[] communicantAddressBytes = communicant.Value.Address.GetAddressBytes();
                    for (int i = 0; i < netAddressBytes.Length; i++)
                    {
                        netAddressBytes[i] = communicantAddressBytes[i];
                    }
                    break;
                }
            }
            foreach (var ip in
                from address in Dns.GetHostAddresses(Dns.GetHostName())
                where address.AddressFamily == AddressFamily.InterNetwork
                select address)
            {
                byte[] ipAddressBytes = ip.GetAddressBytes();
                bool fromSameNet = true;
                for (int i = 0; i < netAddressBytes.Length; i++)
                {
                    if (ipAddressBytes[i] != netAddressBytes[i])
                    {
                        fromSameNet = false;
                        break;
                    }
                }
                if (fromSameNet)
                {
                    return ip;
                }
            }
            return null;
        }
        protected override void DoRunAsServer()
        {
            while(IsRunning)
            {
                broadcaster.Broadcast(BitConverter.GetBytes(uint.MaxValue), 50, 10);//takes at least 500ms
                Thread.Sleep(500);
            }
        }
        protected override void OnDataReceived(IPEndPoint sender, byte[] data)
        {
            uint senderPerformanceIndex = BitConverter.ToUInt32(data, 0);
            if (communicants.TryGetValue(senderPerformanceIndex, out IPEndPoint storedSender))
            {
                communicants[senderPerformanceIndex] = GetLower(sender, storedSender);
            }
            else
            {
                communicants.Add(senderPerformanceIndex, sender);
            }
        }

        private IPEndPoint GetLower(IPEndPoint ip1, IPEndPoint ip2)
        {
            byte[] ip1Bytes = ip1.Address.GetAddressBytes();
            byte[] ip2Bytes = ip2.Address.GetAddressBytes();
            for (int i = 0; i < ip1Bytes.Length; i++)
            {
                if (ip1Bytes[i] < ip2Bytes[i])
                {
                    return ip1;
                }
                else if (ip1Bytes[i] > ip2Bytes[i])
                {
                    return ip2;
                }
            }
            return ip1;//ip1 equals to up2
        }
    }
}
