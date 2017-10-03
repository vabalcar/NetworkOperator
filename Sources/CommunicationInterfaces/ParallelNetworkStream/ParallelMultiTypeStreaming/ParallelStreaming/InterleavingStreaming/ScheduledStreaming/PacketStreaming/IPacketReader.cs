using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    public interface IPacketReader
    {
        bool PacketTypeIsMatchable { get; }
        bool PacketTypeMatches(byte b);
        Packet ReadPacket(Stream stream);
        void Reset();
    }
}
