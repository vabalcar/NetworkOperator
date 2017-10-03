using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using System.IO;
using System.Threading;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming
{
    public abstract class ScheduledStream : PacketStream
    {
        private IDataSplitter dataSplitter;
        private IPacketProcessor packetProcessor;

        private SemaphoreSlim maxParallelTransfersSemaphore;
        private SemaphoreSlim writeSemaphore = new SemaphoreSlim(1);
        private SemaphoreSlim writeNextSemaphore = new SemaphoreSlim(0);
        private int writeSemaphoreWaiters = 0;
        private int writeNextSemaphoreWaiters = 0;

        protected ScheduledStream(IStreamScheduler scheduler)
        {
            dataSplitter = scheduler.DataSplitter;
            packetProcessor = scheduler.PacketProcessor;
            maxParallelTransfersSemaphore = new SemaphoreSlim(scheduler.MaxParallelTransfers);
        }
        protected void Write(byte[] data)
        {
            Packet[] packets = dataSplitter.Split(data);
            foreach (var item in packets)
            {
                Write(item);
            }
            /*
            maxParallelTransfersSemaphore.Wait();
            for (int i = 0; i < packets.Length; i++)
            {
                Interlocked.Increment(ref writeSemaphoreWaiters);
                writeSemaphore.Wait();
                Interlocked.Decrement(ref writeSemaphoreWaiters);
                Write(packets[i]);
                if (i + 1 != packets.Length)
                {
                    Interlocked.Increment(ref writeNextSemaphoreWaiters);
                }
                if (writeSemaphoreWaiters == 0 && writeNextSemaphoreWaiters > 0)
                {
                    writeNextSemaphore.Release();
                }
                else
                {
                    writeSemaphore.Release();
                }
                if (i + 1 != packets.Length)
                {
                    writeNextSemaphore.Wait();
                    Interlocked.Decrement(ref writeNextSemaphoreWaiters);
                }
                else
                {
                    break;
                }
                if (writeNextSemaphoreWaiters == 0)
                {
                    writeSemaphore.Release();
                }
                else
                {
                    writeNextSemaphore.Release();
                }
            }*/
            maxParallelTransfersSemaphore.Release();
        }
        public byte[] Read()
        {
            byte[] readData;
            while ((readData = packetProcessor.Process(ReadPacket())) == null) ;
            return readData;
        }
    }
}
