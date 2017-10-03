using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets
{
    class SmallDataPacket : Packet
    {
        public const byte TYPE = 2;
        public const int DATA_LENGTH_START_INDEX = 1;
        public const int DATA_START_INDEX = 3;
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

        public SmallDataPacket(byte[] data) : base(DATA_START_INDEX + data.Length)
        {
            Type = TYPE;
            DataLength = (short)data.Length;
            Data = data;
        }

    }
}
