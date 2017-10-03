namespace NetworkOperator.CommunicationInterfaces.Connection
{
    public enum InternalMessageType : byte
    {
        RegistrationRequest, ConnectionRequest, BroacastRequest, ClientList, ClientConnected, ClientDisconnected
    }
    public class InternalMessage
    {
        public InternalMessageType MessageType { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}
