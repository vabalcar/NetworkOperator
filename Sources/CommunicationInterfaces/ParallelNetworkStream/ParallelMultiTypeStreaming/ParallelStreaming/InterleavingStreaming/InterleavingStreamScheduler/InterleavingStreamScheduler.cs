using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingScheduledStreaming.ScheduledStreaming.StreamSchedulers.InterleavingStreamScheduler
{
    class InterleavingStreamScheduler : IStreamScheduler
    {
        public IDataSplitter DataSplitter { get; protected set; } = new InterleavingStreamDataSplitter();
        public IPacketProcessor PacketProcessor { get; protected set; } = new InterleavingStreamPacketProcessor();
        public int MaxParallelTransfers { get; } = byte.MaxValue;
    }
}
