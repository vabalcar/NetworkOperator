using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies.Broadcasting
{
    public class BroadcastClient
    {
        public int ReceiveTimeout { get; set; } = 5;//s

        private const int internalTimeout = 1000;//ms

        private UdpClient client = new UdpClient();
        private Dictionary<IPAddress, bool> bindedAddresses = new Dictionary<IPAddress, bool>();
        private EndPointFactory endPointFactory;
        private SemaphoreSlim internalSemaphore = new SemaphoreSlim(1);
        private bool closed = false;

        public BroadcastClient(int localPort)
        {
            client.Client.ReceiveTimeout = internalTimeout;
            endPointFactory = new EndPointFactory(localPort);
        }

        public void BindIfNecessary(IPAddress localIPAddress)
        {
            internalSemaphore.Wait();
            if (!closed && !bindedAddresses.TryGetValue(localIPAddress, out bool value))
            {
                client.Client.Bind(endPointFactory.Create(localIPAddress));
                bindedAddresses.Add(localIPAddress, true);
            }
            internalSemaphore.Release();
        }

        public void Send(byte[] data)
        {
            internalSemaphore.Wait();
            client.Send(data, data.Length, endPointFactory.BroadcastSend);
            internalSemaphore.Release();
        }

        public IPEndPoint Receive(out byte[] data)
        {
            data = null;
            IPEndPoint sender = endPointFactory.BrodcastReceive;
            int timeoutCount = -1;//increased to 0 immidiately
            while (timeoutCount++ < ReceiveTimeout)
            {
                try
                {
                    data = client.Receive(ref sender);
                    return sender;
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        if (timeoutCount == ReceiveTimeout)
                        {
                            throw;
                        }
                        continue;
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
            return null;
        }

        public void Close()
        {
            closed = true;
            client.Close();
            bindedAddresses.Clear();
        }
    }
}
