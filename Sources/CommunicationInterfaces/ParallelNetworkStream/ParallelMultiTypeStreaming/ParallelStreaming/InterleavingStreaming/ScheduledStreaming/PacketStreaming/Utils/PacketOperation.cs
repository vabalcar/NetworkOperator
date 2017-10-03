using System;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils
{
    static class PacketOperation
    {
        public static void Store(byte[] packet, int startIndex, short integer)
        {
            byte[] converted = BitConverter.GetBytes(integer);
            for (int i = 0; i < converted.Length; i++)
            {
                packet[startIndex + i] = converted[i];
            }
        }
        public static void Store(byte[] packet, int startIndex, int integer)
        {
            byte[] converted = BitConverter.GetBytes(integer);
            for (int i = 0; i < converted.Length; i++)
            {
                packet[startIndex + i] = converted[i];
            }
        }
        public static byte[] GetDataCopy(byte[] packet, int dataStartIndex)
        {
            byte[] data = new byte[packet.Length - dataStartIndex];
            Array.Copy(packet, dataStartIndex, data, 0, data.Length);
            return data;
        }
    }
}
