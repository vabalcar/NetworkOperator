using NetworkOperator.DataStructures.ThreadSafe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace NetworkOperator.CommunicationInterfaces.Connection
{
    class Client : IDisposable
    {
        private const string SERVER_SIDE_CLIENT_NAME = "s";
        private class BroadcastListEnumerator : IEnumerable<ParallelNetworkStream>
        {
            private Client parent;

            private ShareableImmutableDictionary<string, Client> registerOfClients;

            public BroadcastListEnumerator(Client parent, ShareableImmutableDictionary<string, Client> registerOfClients)
            {
                this.parent = parent;
                this.registerOfClients = registerOfClients;
            }
            public IEnumerator<ParallelNetworkStream> GetEnumerator()
            {
                foreach (var client in ReadClients())
                {
                    yield return client.stream;
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public override string ToString()
            {
                StringBuilder clientListBuider = new StringBuilder();
                foreach (var client in ReadClients())
                {
                    clientListBuider.AppendLine(client.ToString());
                }
                return clientListBuider.ToString();
            }
            private IEnumerable<Client> ReadClients()
            {
                foreach (var client in registerOfClients)
                {
                    if (client == parent)
                    {
                        continue;
                    }
                    yield return client;
                }
            }
        }

        public event Action<InternalMessage> OnClientRequestReceived
        {
            add
            {
                stream.AddReceivedDataHandler(value);
            }
            remove
            {
                stream.RemoveReceivedDataHandler(value);
            }
        }
        public ParallelNetworkStream.ServerOperator Operations { get; private set; }
        public string Name { get; private set; }
        public IPAddress Address { get; private set; }
        public bool IsConnected => stream.IsOpened;

        private ParallelNetworkStream stream;
        private ShareableImmutableDictionary<string, Client> register;
        private Action completeIntroduction;

        public Client(Socket clientSocket)
        {
            stream = new ParallelNetworkStream(clientSocket)
            {
                LocalHostName = SERVER_SIDE_CLIENT_NAME,
                BroadcastEverything = false,
                EnableUnreliableTransfers = false
            };
            stream.OnConnectionEnded += Dispose;
            Operations = stream.ServerOperations;
        }
        public static Client operator+(Client client1, Client client2)
        {
            client1.Operations.Join(client2.stream);
            return client1;
        }
        public static Client operator~(Client client)
        {
            client.Operations.Disjoin();
            return client;
        }
        public static implicit operator string (Client client) => client.Name;
        public void Connect()
        {
            completeIntroduction = stream.Open(false);
            Name = stream.RemoteHostName;
            if (IPAddress.IsLoopback(stream.RemoteHostIPAddress))
            {
                Address = Server.Current.Address;
            }
            else
            {
                Address = stream.RemoteHostIPAddress;
            }
        }
        public void Register(ShareableImmutableDictionary<string, Client> register)
        {
            this.register = register;
            var broadcastListEnumerator = new BroadcastListEnumerator(this, this.register);
            stream.BroadcastList = broadcastListEnumerator;
            register.Add(Name, this);
            SendMessage(new InternalMessage()
            {
                MessageType = InternalMessageType.ClientList,
                Sender = SERVER_SIDE_CLIENT_NAME,
                Content = broadcastListEnumerator.ToString()
            });
            foreach (var client in this.register)
            {
                if (client == this)
                {
                    continue;
                }
                client.SendMessage(new InternalMessage()
                {
                    MessageType = InternalMessageType.ClientConnected,
                    Sender = SERVER_SIDE_CLIENT_NAME,
                    Content = ToString()
                });
            }
            completeIntroduction();
            completeIntroduction = null;
        }
        public void SendMessage(InternalMessage message) => stream.Write(message);
        public void Dispose()
        {
            register.Remove(Name);
            foreach (var client in register)
            {
                client.SendMessage(new InternalMessage()
                {
                    MessageType = InternalMessageType.ClientDisconnected,
                    Sender = SERVER_SIDE_CLIENT_NAME,
                    Content = Name
                });
            }
            stream.Dispose();
        }
        public override string ToString() => $"{Name}@{Address}";
    }
}
