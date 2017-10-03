using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets.ManagePackets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.PacketReaders
{
    class NewTransferPacketReader : IPacketReader
    {
        private int readingIndex = 0;
        public bool PacketTypeIsMatchable { get; private set; } = true;

        public bool PacketTypeMatches(byte b)
        {
            switch (readingIndex++)
            {
                case 0:
                    if (b == ManagePacket.TYPE)
                    {
                        return false;
                    }
                    goto default;
                case 1:
                    if (b == NewTransferPacket.ARG)
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

            int? transferLengthNullable = StreamOperation.ReadShort(stream);
            if (transferLengthNullable == null)
            {
                return null;
            }
            int transferLength = (int)transferLengthNullable;

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

            return new NewTransferPacket(transferId, transferLength, data);
        }

        public void Reset()
        {
            readingIndex = 0;
            PacketTypeIsMatchable = true;
        }
    }
}
