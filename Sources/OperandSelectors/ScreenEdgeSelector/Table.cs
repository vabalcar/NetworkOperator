using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkOperator.OperandSelectors
{
    public class Table : Grid, IEnumerable<Cell>
    {
        public event Action<Cell> OnClick
        {
            add
            {
                foreach (var cell in this)
                {
                    cell.Border.MouseLeftButtonDown += (sender, args) => value((sender as FrameworkElement).Parent as Cell);
                }
            }
            remove
            {
                throw new NotImplementedException();
            }
        }
        public new event Action<Cell> OnMouseEnter
        {
            add
            {
                foreach (var cell in this)
                {
                    cell.Border.MouseEnter += (sender, args) => value((sender as FrameworkElement).Parent as Cell);
                }
            }
            remove
            {
                throw new NotImplementedException();
            }
        }
        public new event Action<Cell> OnMouseLeave
        {
            add
            {
                foreach (var cell in this)
                {
                    cell.Border.MouseLeave += (sender, args) => value((sender as FrameworkElement).Parent as Cell);
                }
            }
            remove
            {
                throw new NotImplementedException();
            }
        }
        private int rows;
        public int Rows
        {
            get => rows;
            set
            {
                rows = value;
                Create();
            }
        }
        public int MinRowHeight
        {
            set
            {
                foreach (var rowDefinition in RowDefinitions)
                {
                    rowDefinition.MinHeight = value;
                }
            }
        }
        private int columns;
        public int Columns
        {
            get => columns;
            set
            {
                columns = value;
                Create();
            }
        }
        public int MinColumnWidth
        {
            set
            {
                foreach (var columnDefinition in ColumnDefinitions)
                {
                    columnDefinition.MinWidth = value;
                }
            }
        }
        public Thickness CellPadding
        {
            set
            {
                foreach (var cell in this)
                {
                    cell.Border.Padding = value;
                }
            }
        }
        public Cell this[int y, int x]
        {
            get => Children[y * Columns + x] as Cell;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (y < 0 || y > Rows || x < 0 || x > Columns)
                {
                    throw new IndexOutOfRangeException();
                }
                if (y * Columns + x == Children.Count)
                {
                    SetRow(value, y);
                    SetColumn(value, x);
                    Children.Add(value);
                }
                else
                {
                    var changedCell = this[y, x];
                    changedCell.Content = value.Content;
                    changedCell.BackgroundColor = value.BackgroundColor;
                }
            }
        }
        public Table SubTable(int x, int y, int rows, int columns)
        {
            Table subTable = new Table()
            {
                Rows = rows,
                Columns = columns
            };
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    subTable[i, j] = this[y + i, x + j];
                }
            }
            return subTable;
        }
        public void AssignCells(Table anotherTable)
        {
            for (int i = 0; i < Math.Min(anotherTable.Rows, Rows); i++)
            {
                for (int j = 0; j < Math.Min(anotherTable.Columns, Columns); j++)
                {
                    this[i, j] = anotherTable[i, j];
                }
            }
        }
        public Table()
        {
        }
        private void Create()
        {
            if (Rows == 0 || Columns == 0)
            {
                return;
            }
            for (int i = 0; i < Rows; i++)
            {
                RowDefinitions.Add(new RowDefinition()
                {
                    Height = GridLength.Auto
                });
            }
            for (int i = 0; i < Columns; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = GridLength.Auto
                });
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Thickness thickness = new Thickness(1,1,0,0);
                    if (i == Rows - 1)
                    {
                        thickness.Bottom = 1;
                    }
                    if (j == Columns - 1)
                    {
                        thickness.Right = 1;
                    }
                    var cell = new Cell(new Border() { BorderBrush = Brushes.Black, BorderThickness = thickness })
                    {
                        X = j,
                        Y = i
                    };
                    this[i, j] = cell;
                }
            }
        }
        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                yield return Children[i] as Cell;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
