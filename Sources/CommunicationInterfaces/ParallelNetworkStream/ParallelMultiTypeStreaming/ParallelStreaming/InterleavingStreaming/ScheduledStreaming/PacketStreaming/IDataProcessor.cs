namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    interface IDataProcessor
    {
        void Process(byte[] data);
    }
}
