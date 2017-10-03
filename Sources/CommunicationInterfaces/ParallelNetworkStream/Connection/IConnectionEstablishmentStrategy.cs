using System;
using System.Net;

namespace NetworkOperator.CommunicationInterfaces.Connection
{
    public interface IConnectionEstablishmentStrategy : IDisposable
    {
        int LocalPort { get; }
        int RemotePort { get; }
        bool IsLocalHostServer { get; }
        IPAddress ServerIPAddress { get; }
        bool MultipleClients { get; }
        TcpRole Role { get; set; }
        bool IsRunning { get; }
        void Run();
        void Stop();
    }
    public enum TcpRole
    {
        Client, Server
    }
}
