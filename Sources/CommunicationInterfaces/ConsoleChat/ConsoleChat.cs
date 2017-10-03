using NetworkOperator.CommunicationInterfaces;
using NetworkOperator.CommunicationInterfaces.Connection;
using System;
using System.IO;
using System.Text;

namespace ConsoleChat
{
    class ConsoleChatter
    {
        public static void Run(IConnectionEstablishmentStrategy connectionEstablishmentStrategy, int timeout, 
            Reliability messageTransferReliability)
        {
            TextReader input = Console.In;
            TextWriter output = Console.Out;
            ParallelNetworkStream stream = new ParallelNetworkStream(connectionEstablishmentStrategy) { Timeout = timeout };
            stream.AddReceivedDataHandler<string>(message => output.WriteLine(message));
            stream.OnInternalErrorDetected += () => output.WriteLine("Internal problem on local host has been detected.");
            stream.OnConnectionInterrupted += () => output.WriteLine("Connection was interrupted.");
            stream.OnRemoteHostDisconnected += () => output.WriteLine("Remote host has disconnected.");
            stream.OnConnectionEnded += () => output.Write("Press enter to end the program . . . ");
            stream.OnConnectionEnded += () => stream.Dispose();

            void CancelConnectingHandler(object sender, ConsoleCancelEventArgs args)
            {
                if (!stream.IsOpened)
                {
                    args.Cancel = true;//Don't terminate the process.
                    stream.CancelOpenning();
                    output.WriteLine("Connecting has been canceled.");
                }
            }

            void CancelHandler(object sender, ConsoleCancelEventArgs args)
            {
                stream.Dispose();
            }

            Console.CancelKeyPress += CancelConnectingHandler;//Cancel connecting by pressing Ctrl + C.
            Console.CancelKeyPress += CancelHandler;

            output.WriteLine("Connecting...");
            stream.Open();

            if (stream.IsOpened)
            {
                Console.CancelKeyPress -= CancelConnectingHandler;
                output.WriteLine($"Connection established with {stream.RemoteHostIPAddress}.");
                string line;

                while (stream.IsOpened && (line = input.ReadLine()) != null)//Stop chatting by pressing Ctrl + Z.
                {
                    string[] tokens = line.Split(' ');
                    switch (tokens[0])
                    {
                        case "clientList":
                            output.WriteLine($"{nameof(stream.ClientRequests.ConnectableClients)}:");
                            int contactNumber = 0;
                            foreach (var client in stream.ClientRequests.ConnectableClients)
                            {
                                output.WriteLine($"({contactNumber++}) {client}");
                            }
                            break;
                        case "connectWith":
                            if (tokens.Length > 1)
                            {
                                string clientToConnectName = string.Empty;
                                if (tokens.Length == 2 
                                    && int.TryParse(tokens[1], out int selectedCommunicant))
                                {
                                    if (selectedCommunicant > stream.ClientRequests.ConnectableClients.Count)
                                    {
                                        Console.WriteLine("Selected client doesn't exist.");
                                        continue;
                                    }
                                    int i = 0;
                                    foreach (var communicant in stream.ClientRequests.ConnectableClients.Keys)
                                    {
                                        if (i++ == selectedCommunicant)
                                        {
                                            clientToConnectName = communicant;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    StringBuilder clientNameBuilder = new StringBuilder();
                                    for (int i = 1; i < tokens.Length; i++)
                                    {
                                        if (i != 1)
                                        {
                                            clientNameBuilder.Append(' ');
                                        }
                                        clientNameBuilder.Append(tokens[i]);
                                    }
                                    clientToConnectName = clientNameBuilder.ToString();
                                }

                                if (stream.ClientRequests.ConnectableClients.ContainsKey(clientToConnectName))
                                {
                                    stream.ClientRequests.ConnectWith(clientToConnectName, messageTransferReliability);
                                    output.WriteLine($"Client {clientToConnectName} has been connected successfully.");
                                }
                                else
                                {
                                    output.WriteLine($"Client {clientToConnectName} is not connectable.");
                                }
                            }
                            break;
                        case "startBroadcast":
                            stream.ClientRequests.BroadcastEverything = true;
                            output.WriteLine("Broadcast has started.");
                            break;
                        case "stopBroadcast":
                            stream.ClientRequests.BroadcastEverything = false;
                            output.WriteLine("Broadcast has stopped.");
                            break;
                        default:
                            if (line.Length != 0)
                            {
                                stream.Write($"{stream.LocalHostName}: {line}");
                            }
                            break;
                    }
                }
            }
            stream.Close();
        }
    }
}
