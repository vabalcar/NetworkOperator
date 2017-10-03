using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets;
using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets.ManagePackets;
using NetworkOperator.DataStructures.ThreadSafe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming
{
    class InterleavingStreamPacketProcessor : IPacketProcessor
    {
        private byte[][] transfers;
        private int[] offsets;
        private Dictionary<byte, byte> ids = new Dictionary<byte, byte>();
        private LockingQueue<byte> freeIds = new LockingQueue<byte>();
        public int MaxParallelTransfers { get; private set; } = byte.MaxValue + 1;

        public InterleavingStreamPacketProcessor()
        {
            transfers = new byte[MaxParallelTransfers][];
            offsets = new int[MaxParallelTransfers];
            foreach (var id in Enumerable.Range(0, MaxParallelTransfers))
            {
                freeIds.Enqueue((byte)(MaxParallelTransfers - id));
            }
        }

        public byte[] Process(Packet readPacket)
        {
            if (readPacket is NewTransferPacket newTransferPacket)
            {
                if (freeIds.IsEmpty())
                {
                    return null;
                }
                byte localId = freeIds.Dequeue();
                byte transferId = newTransferPacket.TransferId;
                if (ids.ContainsKey(transferId) && ids[transferId] == 0 || transfers[localId] != null)
                {
                    return null;
                }
                ids[transferId] = localId;
                transfers[localId] = new byte[newTransferPacket.TransferLength];
                return Process(new DataPacket(transferId, newTransferPacket.Data));
            }
            else if (readPacket is DataPacket dataPacket)
            {
                byte localId = ids[dataPacket.TransferId];
                Array.Copy(dataPacket.Data, 0, transfers[localId], offsets[localId], dataPacket.DataLength);
                offsets[localId] += dataPacket.DataLength;
                if (offsets[localId] == transfers[localId].Length - 1)
                {
                    byte[] readData = transfers[localId];
                    transfers[localId] = null;
                    offsets[localId] = 0;
                    freeIds.Enqueue(localId);
                    ids[localId] = 0;
                    return readData;
                }
            }
            else if (readPacket is SmallDataPacket smallDataPacket)
            {
                return smallDataPacket.Data;
            }
            return null;
        }
    }
}
