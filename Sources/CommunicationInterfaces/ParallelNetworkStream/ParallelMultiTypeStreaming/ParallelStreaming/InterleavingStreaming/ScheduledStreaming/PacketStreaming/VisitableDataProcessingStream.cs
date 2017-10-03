using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    public abstract class VisitableDataProcessingStream : VisitableStream, IDataProcessor
    {
        public abstract void Open(Stream stream);
        public abstract void Process(byte[] data);
    }
}
