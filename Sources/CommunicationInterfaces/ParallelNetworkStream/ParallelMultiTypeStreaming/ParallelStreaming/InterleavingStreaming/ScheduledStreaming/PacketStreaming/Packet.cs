using System;
using System.Text;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming
{
    public abstract class Packet
    {
        public const int TYPE_POSITION = 0;
        public const int MAX_PACKET_LENGTH = short.MaxValue;

        public byte[] RawData { get; private set; }
        public int Length
        {
            get
            {
                return RawData.Length;
            }
        }
        public byte Type
        {
            get
            {
                return RawData[TYPE_POSITION];
            }
            protected set
            {
                RawData[TYPE_POSITION] = value;
            }
        }
        public Packet(int length)
        {
            if (length > MAX_PACKET_LENGTH)
            {
                throw new ArgumentException($"Packet length must be lesser or equal to {MAX_PACKET_LENGTH}.");
            }
            RawData = new byte[length];
        }
        public override string ToString()
        {
            StringBuilder packetInString = new StringBuilder();
            packetInString.Append('[');
            for (int i = 0; i < RawData.Length; i++)
            {
                if (i + 1 != RawData.Length)
                {
                    packetInString.Append($"{RawData[i]};");
                }
                else
                {
                    packetInString.Append(RawData[i]);
                }
            }
            packetInString.Append(']');
            return packetInString.ToString();
        }
    }
}
