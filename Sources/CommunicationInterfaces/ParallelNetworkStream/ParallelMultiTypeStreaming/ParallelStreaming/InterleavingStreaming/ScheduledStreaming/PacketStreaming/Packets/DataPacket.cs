using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets
{
    class DataPacket : Packet
    {
        public const byte TYPE = 1;
        public const int TRANSFER_ID_INDEX = 1;
        public const int DATA_LENGTH_START_INDEX = 2;
        public const int DATA_START_INDEX = 6;

        public byte TransferId
        {
            get
            {
                return RawData[TRANSFER_ID_INDEX];
            }
            set
            {
                RawData[TRANSFER_ID_INDEX] = value;
            }
        }
        public short DataLength
        {
            get
            {
                return BitConverter.ToInt16(RawData, DATA_LENGTH_START_INDEX);
            }
            private set
            {
                PacketOperation.Store(RawData, DATA_LENGTH_START_INDEX, value);
            }
        }
        public byte[] Data
        {
            get
            {
                return PacketOperation.GetDataCopy(RawData, DATA_START_INDEX);
            }
            private set
            {
                Array.Copy(value, 0, RawData, DATA_START_INDEX, value.Length);
            }
        }

        public DataPacket(byte transferId, byte[] data) : base(DATA_START_INDEX + data.Length)
        {
            Type = TYPE;
            TransferId = transferId;
            DataLength = (short)data.Length;
            Data = data;
        }
    }
}
