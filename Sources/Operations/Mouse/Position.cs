using System;
using System.IO;

namespace NetworkOperator.IO
{
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static Movement operator - (Position position1, Position position2) 
            => new Movement()
            {
                X = (short)(position1.X - position2.X),
                Y = (short)(position1.Y - position2.Y)
            };
        
        public static Position operator + (Position position, Movement movement)
            => new Position()
            {
                X = position.X + movement.X,
                Y = position.Y + movement.Y
            };

        public byte[] Serialize()
        {
            MemoryStream serialized = new MemoryStream();
            foreach (var b in BitConverter.GetBytes(X)) serialized.WriteByte(b);
            foreach (var b in BitConverter.GetBytes(Y)) serialized.WriteByte(b);
            return serialized.ToArray();
        }
        public static Position Deserialize(byte[] bytes)
        {
            int tmp = 0;
            return Deserialize(bytes, ref tmp);
        }
        public static Position Deserialize(byte[] bytes, ref int offset)
        {
            var position = new Position();
            position.X = BitConverter.ToInt32(bytes, offset); offset += 4;
            position.Y = BitConverter.ToInt32(bytes, offset); offset += 4;
            return position;
        }
        public override string ToString()
        {
            return $"{nameof(Position)} [{nameof(X)}: {X}, {nameof(Y)}: {Y}]";
        }
    }
}
