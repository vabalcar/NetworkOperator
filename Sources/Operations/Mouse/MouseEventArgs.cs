using System;
using System.IO;
using System.Text;

namespace NetworkOperator.IO
{
    public class MouseEventArgs
    {
        public bool IsSimulated { get; set; }
        public uint Timestamp { get; set; }
        public Position Position { get; set; }
        public MouseEventType EventType { get; set; }
        public MouseButton Button { get; set; } = MouseButton.None;
        public MouseButtonState ButtonState { get; set; } = MouseButtonState.None;
        public MouseWheel Wheel { get; set; } = MouseWheel.None;
        public MouseWheelRotationDirection WheelRotationDirection { get; set; } = MouseWheelRotationDirection.None;
        public short WheelRotationLength { get; set; } = -1;
        public bool Cancel { get; set; } = false;

        public byte[] Serialize()
        {
            MemoryStream serialized = new MemoryStream();
            foreach (var b in BitConverter.GetBytes(IsSimulated)) serialized.WriteByte(b);
            foreach (var b in BitConverter.GetBytes(Timestamp)) serialized.WriteByte(b);
            foreach (var b in Position.Serialize()) serialized.WriteByte(b);
            serialized.WriteByte((byte)EventType);
            serialized.WriteByte((byte)Button);
            serialized.WriteByte((byte)ButtonState);
            serialized.WriteByte((byte)Wheel);
            serialized.WriteByte((byte)WheelRotationDirection);
            foreach (var b in BitConverter.GetBytes(WheelRotationLength)) serialized.WriteByte(b);
            foreach (var b in BitConverter.GetBytes(Cancel)) serialized.WriteByte(b);
            return serialized.ToArray();
        }
        public static MouseEventArgs Deserialize(byte[] bytes)
        {
            int tmp = 0;
            return Deserialize(bytes, ref tmp);
        }
        public static MouseEventArgs Deserialize(byte[] bytes, ref int offset)
        {
            var mouseEventArgs = new MouseEventArgs();
            mouseEventArgs.IsSimulated = BitConverter.ToBoolean(bytes, offset++);
            mouseEventArgs.Timestamp = BitConverter.ToUInt32(bytes, offset); offset += 4;
            mouseEventArgs.Position = Position.Deserialize(bytes, ref offset);
            mouseEventArgs.EventType = (MouseEventType)bytes[offset++];
            mouseEventArgs.Button = (MouseButton)bytes[offset++];
            mouseEventArgs.ButtonState = (MouseButtonState)bytes[offset++];
            mouseEventArgs.Wheel = (MouseWheel)bytes[offset++];
            mouseEventArgs.WheelRotationDirection = (MouseWheelRotationDirection)bytes[offset++];
            mouseEventArgs.WheelRotationLength = BitConverter.ToInt16(bytes, offset); offset += 2;
            mouseEventArgs.Cancel = BitConverter.ToBoolean(bytes, offset++);
            return mouseEventArgs;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (EventType)
            {
                case MouseEventType.Movement:
                    sb.Append($"Mouse moved to {Position}");
                    break;
                case MouseEventType.ButtonStateChanged:
                    sb.Append($"{Button} {nameof(Button)} {ButtonState} at {Position}");
                    break;
                case MouseEventType.WheelRotated:
                    sb.Append($"{Wheel} {nameof(Wheel)} rotated {WheelRotationDirection} by {WheelRotationLength}");
                    break;
                default:
                    return base.ToString();
            }
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
