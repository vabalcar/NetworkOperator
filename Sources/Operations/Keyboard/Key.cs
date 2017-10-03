using System;
using System.Text;

namespace NetworkOperator.IO
{
    public class Key : IEquatable<Key>
    {
        public bool IsMouseKey
        {
            get => VirtualKey >= VirtualKey.LBUTTON && VirtualKey <= VirtualKey.RBUTTON
                    || VirtualKey >= VirtualKey.MBUTTON && VirtualKey <= VirtualKey.XBUTTON2;
        }
        public bool IsNumber
        {
            get => VirtualKey >= VirtualKey._0 && VirtualKey <= VirtualKey._9
                    || VirtualKey >= VirtualKey.NUMPAD0 && VirtualKey <= VirtualKey.NUMPAD9;
        }
        public bool IsLetter
        {
            get => VirtualKey >= VirtualKey.A && VirtualKey <= VirtualKey.Z;
        }
        public bool IsSymbol
        {
            get => VirtualKey >= VirtualKey.MULTIPLY && VirtualKey <= VirtualKey.DIVIDE;
            
        }
        public bool IsWhiteSpace
        {
            get => VirtualKey == VirtualKey.SPACE 
                    || VirtualKey == VirtualKey.TAB 
                    || VirtualKey == VirtualKey.RETURN;//enter
        }
        public bool IsPrintable
        {
            get => IsNumber || IsLetter || IsSymbol || IsWhiteSpace;
        }
        public byte ID;

        public VirtualKey VirtualKey
        {
            get => (VirtualKey)ID;
            set => ID = (byte)value;
        }

        public bool Equals(Key other) => ID == other.ID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{nameof(Key)} ");
            if (IsPrintable)
            {
                sb.Append($"{(char)ID}");
            }
            else
            {
                sb.Append($"{VirtualKey}");
            }
            return sb.ToString();
        }
    }
}
