using NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils;
using System;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Packets.ManagePackets
{
    class NewTransferPacket : ManagePacket
    {
        public const int TRANSFER_ID_INDEX = 2;
        public const int TRANSFER_LENGTH_START_INDEX = 3;
        public const int DATA_LENGTH_START_INDEX = 7;
        public const int DATA_START_INDEX = 11;

        public const byte ARG = 0;

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
        public int TransferLength
        {
            get
            {
                return BitConverter.ToInt32(RawData, TRANSFER_LENGTH_START_INDEX);
            }
            private set
            {
                PacketOperation.Store(RawData, TRANSFER_LENGTH_START_INDEX, value);
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
        public NewTransferPacket(byte transferId, int transferLength, byte[] data) : base(DATA_START_INDEX + data.Length)
        {
            Arg = ARG;
            TransferLength = transferLength;
            DataLength = (short)data.Length;
            Data = data;
        }
    }
}
