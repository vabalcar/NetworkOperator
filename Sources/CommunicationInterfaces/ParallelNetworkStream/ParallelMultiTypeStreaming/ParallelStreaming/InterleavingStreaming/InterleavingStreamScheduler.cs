using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming
{
    class InterleavingStreamScheduler : IStreamScheduler
    {
        public IDataSplitter DataSplitter { get; protected set; } = new InterleavingStreamDataSplitter();
        public IPacketProcessor PacketProcessor { get; protected set; } = new InterleavingStreamPacketProcessor();
        public int MaxParallelTransfers { get; } = byte.MaxValue;
    }
}
