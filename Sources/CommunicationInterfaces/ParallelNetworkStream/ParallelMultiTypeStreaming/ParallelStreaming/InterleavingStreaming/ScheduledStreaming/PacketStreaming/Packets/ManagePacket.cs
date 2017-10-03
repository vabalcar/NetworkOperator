namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets
{
    abstract class ManagePacket : Packet
    {
        public const int ARG_INDEX = 1;
        public const byte TYPE = 0;
        public byte Arg
        {
            get
            {
                return RawData[ARG_INDEX];
            }
            protected set
            {
                RawData[ARG_INDEX] = value;
            }
        }
        public ManagePacket(int length) : base(length)
        {
            Type = TYPE;
        }
    }
}
