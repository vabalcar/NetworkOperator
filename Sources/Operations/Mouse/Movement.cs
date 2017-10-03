using System;
using System.IO;

namespace NetworkOperator.IO
{
    public class Movement
    {
        public short X { get; set; } = 0;
        public short Y { get; set; } = 0;
        public static Movement operator + (Movement movement1, Movement movement2) 
            => new Movement()
            {
                X = (short)(movement1.X + movement2.X),
                Y = (short)(movement1.Y + movement2.Y)
            };
        public byte[] Serialize()
        {
            MemoryStream serialized = new MemoryStream();
            foreach (var b in BitConverter.GetBytes(X)) serialized.WriteByte(b);
            foreach (var b in BitConverter.GetBytes(Y)) serialized.WriteByte(b);
            return serialized.ToArray();
        }
        public static Movement Deserialize(byte[] bytes)
        {
            int tmp = 0;
            return Deserialize(bytes, ref tmp);
        }
        public static Movement Deserialize(byte[] bytes, ref int offset)
        {
            var movement = new Movement();
            movement.X = BitConverter.ToInt16(bytes, offset); offset += 2;
            movement.Y = BitConverter.ToInt16(bytes, offset); offset += 2;
            return movement;
        }
        public override string ToString()
        {
            return $"{nameof(Movement)} [{nameof(X)}: {X}, {nameof(Y)}: {Y}]";
        }
    }
}
