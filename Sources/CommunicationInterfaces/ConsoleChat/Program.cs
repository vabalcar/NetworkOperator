using NetworkOperator.CommunicationInterfaces;
using NetworkOperator.CommunicationInterfaces.Connection;
using NetworkOperator.CommunicationInterfaces.Connection.ConnectionEstablishmentStrategies.BroadcastConnectionEstablishmentStrategies;
using System;

namespace ConsoleChat
{
    class Program
    {
        private const int PORT = 5000;
        static void Main(string[] args)
        {
            IConnectionEstablishmentStrategy connectionEstablishmentStrategy;
            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Argument error");
                return;
            }
            connectionEstablishmentStrategy = new HighPerformanceServerStrategy(PORT);
            Reliability messageTransferReliability = Reliability.Reliable;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i][1])
                {
                    case 'c':
                        connectionEstablishmentStrategy = new AutoSearchStrategy(PORT) { Role = TcpRole.Client };
                        break;
                    case 's':
                        connectionEstablishmentStrategy = new AutoSearchStrategy(PORT) { Role = TcpRole.Server };
                        break;
                    case 'r':
                        messageTransferReliability = Reliability.Reliable;
                        break;
                    case 'u':
                        messageTransferReliability = Reliability.Unreliable;
                        break;
                    default:
                        Console.WriteLine("Argument error");
                        return;
                }
            }
            ConsoleChatter.Run(connectionEstablishmentStrategy, 5000, messageTransferReliability);
        }
    }
}
