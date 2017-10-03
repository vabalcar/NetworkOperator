using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.PacketReaders;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming
{
    public abstract class InterleavingStream : ScheduledStream
    {
        protected InterleavingStream() : base(new InterleavingStreamScheduler())
        {
            RegisterPacketReader(new NewTransferPacketReader());
            RegisterPacketReader(new DataPacketReader());
            RegisterPacketReader(new SmallDataPacketReader());
        }
    }
}
