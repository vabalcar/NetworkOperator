using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkOperator.OperandSelectors
{
    public class Cell : Decorator, IEnumerable<UIElement>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsEmpty => Content.Children.Count == 0;
        public Border Border { get; private set; }
        public StackPanel Content
        {
            get => Border.Child as StackPanel;
            set
            {
                if (Border.Child == null)
                {
                    Border.Child = value;
                    return;
                }
                var contentChildren = (Border.Child as StackPanel).Children;
                contentChildren.Clear();
                for (int i = 0; i < value.Children.Count; i++)
                {
                    var movedChild = value.Children[0];
                    value.Children.RemoveAt(0);
                    contentChildren.Add(movedChild);
                }
            }
        }
        public Color PreviousColor { get; private set; }
        public Color BackgroundColor
        {
            get
            {
                if (Border.Background == null)
                {
                    return Colors.Transparent;
                }
                return ((SolidColorBrush)Border.Background).Color;
            }
            set
            {
                PreviousColor = BackgroundColor;
                Border.Background = new SolidColorBrush(value);
            }
        }
        public Cell(Border border)
        {
            Border = border;
            BackgroundColor = Colors.Transparent;
            Content = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Child = Border;
        }
        public void Add(UIElement element) => Content.Children.Add(element);
        public void AddString(string s, int fontSize, HorizontalAlignment alignment) 
            => Add(new TextBlock() { Text = s, FontSize = fontSize, HorizontalAlignment = alignment });
        public void Remove(UIElement element) => Content.Children.Remove(element);
        public void RemoveString(string s)
        {
            var children = Content.Children;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is TextBlock textBlock && textBlock.Text == s)
                {
                    children.RemoveAt(i);
                }
            }
        }
        public void Clear() => Content.Children.Clear();
        public IEnumerator<UIElement> GetEnumerator()
        {
            var children = Content.Children;
            for (int i = 0; i < children.Count; i++)
            {
                yield return children[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => $"{nameof(Cell)} at [{Y},{X}]";
    }
}
