using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.PacketReaders
{
    class DataPacketReader : IPacketReader
    {
        private int readingIndex = 0;
        public bool PacketTypeIsMatchable { get; private set; } = true;

        public bool PacketTypeMatches(byte b)
        {
            switch (readingIndex++)
            {
                case 0:
                    if (b == DataPacket.TYPE)
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
            int readValue = stream.ReadByte();
            if (readValue == -1)
            {
                return null;
            }
            byte transferId = (byte)readValue;

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

            return new DataPacket(transferId, data);
        }

        public void Reset()
        {
            readingIndex = 0;
            PacketTypeIsMatchable = true;
        }
    }
}
