using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    public abstract class PacketStream : VisitableDataProcessingStream
    {
        private Stream stream;
        private PacketReader packetReader = new PacketReader();
        
        public override void Open(Stream stream)
        {
            this.stream = stream;
        }

        protected void RegisterPacketReader(IPacketReader reader) => packetReader.RegisterReader(reader);

        protected Packet ReadPacket()
        {
            Packet readPacket = packetReader.ReadPacket(stream);
            //Console.WriteLine($"reading {readPacket}");
            return readPacket;
        }

        protected void Write(Packet packet)
        {
            //Console.WriteLine($"writing {packet}");
            stream.Write(packet.RawData, 0, packet.Length);
            stream.Flush();
        }
    }
}
