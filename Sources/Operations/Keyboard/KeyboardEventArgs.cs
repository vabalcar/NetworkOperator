using System;
using System.IO;
using System.Text;

namespace NetworkOperator.IO
{
    public class KeyboardEventArgs
    {
        public bool IsSimulated { get; set; }
        public uint Timestamp { get; set; }
        public Key Key { get; set; }
        public KeyType KeyType { get; set; }
        public KeyStatus KeyStatus { get; set; }
        public bool Cancel { get; set; }

        public byte[] Serialize()
        {
            MemoryStream serialized = new MemoryStream();
            foreach (var b in BitConverter.GetBytes(IsSimulated)) serialized.WriteByte(b);
            foreach (var b in BitConverter.GetBytes(Timestamp)) serialized.WriteByte(b);
            serialized.WriteByte(Key.ID);
            serialized.WriteByte((byte)KeyType);
            serialized.WriteByte((byte)KeyStatus);
            foreach (var b in BitConverter.GetBytes(Cancel)) serialized.WriteByte(b);
            return serialized.ToArray();
        }
        public static KeyboardEventArgs Deserialize(byte[] bytes)
        {
            int tmp = 0;
            return Deserialize(bytes, ref tmp);
        }
        public static KeyboardEventArgs Deserialize(byte[] bytes, ref int offset)
        {
            var keyboardEventArgs = new KeyboardEventArgs();
            keyboardEventArgs.IsSimulated = BitConverter.ToBoolean(bytes, offset++);
            keyboardEventArgs.Timestamp = BitConverter.ToUInt32(bytes, offset); offset += 4;
            keyboardEventArgs.Key = new Key() { ID = bytes[offset++] };
            keyboardEventArgs.KeyType = (KeyType)bytes[offset++];
            keyboardEventArgs.KeyStatus = (KeyStatus)bytes[offset++];
            keyboardEventArgs.Cancel = BitConverter.ToBoolean(bytes, offset++);
            return keyboardEventArgs;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{KeyType} {Key} {KeyStatus}");
            if (IsSimulated)
            {
                sb.Append(" - simulated");
            }
            if (Cancel)
            {
                sb.Append(" - canceled");
            }
            return sb.ToString();
        }
    }
}
