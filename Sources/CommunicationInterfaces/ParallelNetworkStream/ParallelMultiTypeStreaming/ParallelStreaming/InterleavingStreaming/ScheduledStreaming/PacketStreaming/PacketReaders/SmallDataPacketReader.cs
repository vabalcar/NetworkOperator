using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.PacketReaders
{
    class SmallDataPacketReader : IPacketReader
    {
        private int readingIndex = 0;
        public bool PacketTypeIsMatchable { get; private set; } = true;

        public bool PacketTypeMatches(byte b)
        {
            switch (readingIndex++)
            {
                case 0:
                    if (b == SmallDataPacket.TYPE)
                    {
                        return true;
                    }
                    goto default;
                default:
                    PacketTypeIsMatchable = false;
                    return false;
            }
        }

        public Packet ReadPacket(Stream stream)
        {
            short? dataLengthNullable = StreamOperation.ReadShort(stream);
            if (dataLengthNullable == null)
            {
                return null;
            }
            short dataLength = (short)dataLengthNullable;

            byte[] data = StreamOperation.ReadBytes(stream, dataLength);
            if (data == null)
            {
                return null;
            }

            return new SmallDataPacket(data);
        }

        public void Reset()
        {
            readingIndex = 0;
            PacketTypeIsMatchable = true;
        }
    }
}
