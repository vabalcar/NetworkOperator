using System.Collections.Generic;
using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    class PacketReader
    {
        private List<IPacketReader> readers = new List<IPacketReader>();

        public void RegisterReader(IPacketReader reader)
        {
            readers.Add(reader);
        }
        public Packet ReadPacket(Stream stream)
        {
            foreach (var reader in readers)
            {
                reader.Reset();
            }
            int readValue;
            byte readByte;
            while ((readValue = stream.ReadByte()) != -1)
            {
                bool failureDetected = true;
                readByte = (byte)readValue;
                foreach (var reader in readers)
                {
                    if (!reader.PacketTypeIsMatchable)
                    {
                        continue;
                    }
                    if (reader.PacketTypeMatches(readByte))
                    {
                        Packet readPacket = reader.ReadPacket(stream);
                        foreach (var readerToReset in readers)
                        {
                            readerToReset.Reset();
                        }
                        return readPacket;
                    }
                    else
                    {
                        failureDetected = false;
                    }
                }
                if (failureDetected) return null;
            }
            return null;
        }
    }
}
