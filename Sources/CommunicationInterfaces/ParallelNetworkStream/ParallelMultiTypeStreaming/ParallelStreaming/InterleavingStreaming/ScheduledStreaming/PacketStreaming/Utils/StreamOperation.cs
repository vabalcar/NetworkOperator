using System;
using System.IO;

namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.ParallelStreaming.InterleavingStreaming.ScheduledStreaming.PacketStreaming.Utils
{
    static class StreamOperation
    {
        public static short? ReadShort(Stream stream)
        {
            byte[] shortBytes = new byte[2];
            if (stream.Read(shortBytes, 0, shortBytes.Length) != shortBytes.Length)
            {
                return null;//typically end of stream reached
            }
            return BitConverter.ToInt16(shortBytes, 0);
        }
        public static int? ReadInt(Stream stream)
        {
            byte[] intBytes = new byte[4];
            if (stream.Read(intBytes, 0, intBytes.Length) != intBytes.Length)
            {
                return null;//typically end of stream reached
            }
            return BitConverter.ToInt32(intBytes, 0);
        }
        public static byte[] ReadBytes(Stream stream, int count)
        {
            byte[] readBytes = new byte[count];
            int readValue;
            for (int i = 0; i < count; i++)
            {
                readValue = stream.ReadByte();
                if (readValue == -1)
                {
                    return null;
                }
                readBytes[i] = (byte)readValue;
            }
            return readBytes;
        }
    }
}
