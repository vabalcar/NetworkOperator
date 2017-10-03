using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets.ManagePackets;
using System;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming
{
    class InterleavingStreamDataSplitter : IDataSplitter
    {
        public int SmallDataBound = 1024;
        private byte lastTransferId = 1;
        public Packet[] Split(byte[] data)
        {
            if (data.Length > SmallDataBound)
            {
                byte transferId = ++lastTransferId;
                byte[][] splittedData = new byte[(int)Math.Ceiling(data.Length / (decimal)SmallDataBound)][];
                for (int i = 0; i < splittedData.Length; i++)
                {
                    if (i + 1 == splittedData.Length)
                    {
                        splittedData[i] = new byte[data.Length - i * SmallDataBound];
                    }
                    else
                    {
                        splittedData[i] = new byte[SmallDataBound];
                    }
                    for (int j = 0; j < splittedData[i].Length; j++)
                    {
                        splittedData[i][j] = data[i * SmallDataBound + j];
                    }
                }
                Packet[] packets = new Packet[splittedData.Length];
                packets[0] = new NewTransferPacket(transferId, data.Length, splittedData[0]);
                for (int i = 1; i < packets.Length; i++)
                {
                    packets[i] = new DataPacket(transferId, splittedData[i]);
                }
                return packets;
            }
            else
            {
                Packet[] packet = new Packet[1];
                packet[0] = new SmallDataPacket(data);
                return packet;
            }
        }
    }
}
