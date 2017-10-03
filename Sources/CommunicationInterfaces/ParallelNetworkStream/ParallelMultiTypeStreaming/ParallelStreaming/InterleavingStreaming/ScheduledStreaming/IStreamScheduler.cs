namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming
{
    public interface IStreamScheduler
    {
        IDataSplitter DataSplitter { get; }
        IPacketProcessor PacketProcessor { get; }
        int MaxParallelTransfers { get; }
    }
}
