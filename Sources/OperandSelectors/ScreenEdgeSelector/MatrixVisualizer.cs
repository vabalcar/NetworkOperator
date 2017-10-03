using System;
using System.Windows;

namespace NetworkOperator.OperandSelectors
{
    public class MatrixVisualizer
    {
        private Action<Cell> previewNonEmptyCellShow;
        public event Action<Cell> PreviewNonEmptyCellShow
        {
            add => previewNonEmptyCellShow += value;
            remove => previewNonEmptyCellShow -= value;
        }
        public int FontSize { get; set; } = 11;
        public HorizontalAlignment TextAlignment { get; set; } = HorizontalAlignment.Left;
        public Table GetVisualization<T>(T[,] data)
        {
            var table = new Table()
            {
                Rows = data.GetLength(0),
                Columns = data.GetLength(1)
            };
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i, j] != null)
                    {
                        table[i, j].AddString(data[i, j].ToString(), FontSize, TextAlignment);
                        previewNonEmptyCellShow(table[i, j]);
                    }
                }
            }
            return table;
        }
    }
}
