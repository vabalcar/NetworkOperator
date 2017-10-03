using System;
using System.Collections.Generic;
using System.Windows;

namespace NetworkOperator.OperandSelectors
{
    public class MatrixView<T>
    {
        public event Action<Cell> PreviewNonEmptyCellShow
        {
            add => visualizer.PreviewNonEmptyCellShow += value;
            remove => visualizer.PreviewNonEmptyCellShow -= value;
        }
        private Action<Cell> previewSwappingCellShow;
        public event Action<Cell> PreviewSwappingCellShow
        {
            add => previewSwappingCellShow += value;
            remove => previewSwappingCellShow -= value;
        }
        private MatrixVisualizer visualizer = new MatrixVisualizer();
        public int FontSize
        {
            get => visualizer.FontSize;
            set => visualizer.FontSize = value;
        }
        public HorizontalAlignment TextAlignment
        {
            get => visualizer.TextAlignment;
            set => visualizer.TextAlignment = value;
        }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        private T[,] matrix;
        public T[,] Matrix
        {
            get
            {
                var copy = new T[matrix.GetLength(0), matrix.GetLength(1)];
                for (int i = 0; i < copy.GetLength(0); i++)
                {
                    for (int j = 0; j < copy.GetLength(1); j++)
                    {
                        copy[i, j] = matrix[i, j];
                    }
                }
                return copy;
            }
        }
        private int swapX = -1;
        private int swapY = -1;
        public MatrixView(T[,] matrix)
        {
            this.matrix = matrix;
        }
        public MatrixView<T> Move(Direction direction) => Move(direction, 1);
        public MatrixView<T> Move(Direction direction, int times)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (Y - times >= 0)
                    {
                        Y -= times;
                    }
                    break;
                case Direction.Down:
                    if (Y + times + Height - 1 < matrix.GetLength(0))
                    {
                        Y += times;
                    }
                    break;
                case Direction.Left:
                    if (X - times >= 0)
                    {
                        X -= times;
                    }
                    break;
                case Direction.Right:
                    if (X + times + Width - 1 < matrix.GetLength(1))
                    {
                        X += times;
                    }
                    break;
                default:
                    break;
            }
            return this;
        }
        public bool EndReached(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Y == 0;
                case Direction.Down:
                    return Y + Height == matrix.GetLength(0);
                case Direction.Left:
                    return X == 0;
                case Direction.Right:
                    return X + Width == matrix.GetLength(1);
                default:
                    return false;
            }
        }
        public void BeginSwap(int y, int x)
        {
            ToRealPosition(ref y, ref x);
            swapX = x;
            swapY = y;
        }
        public MatrixView<T> CompleteSwap(int y, int x)
        {
            ToRealPosition(ref y, ref x);
            var tmp = matrix[swapY, swapX];
            matrix[swapY, swapX] = matrix[y, x];
            matrix[y, x] = tmp;
            swapX = -1;
            swapY = -1;
            return this;
        }
        private void ToRealPosition(ref int y, ref int x)
        {
            y += Y;
            x += X;
        }
        public Table GetView()
        {
            T[,] subMatrix = new T[Height, Width];
            int swapingCellX = -1, swappingCellY = -1;
            bool swappingCellFound = false;
            for (int i = 0; i < subMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < subMatrix.GetLength(1); j++)
                {
                    if (!swappingCellFound && Y + i == swapY && X + j == swapX)
                    {
                        swappingCellFound = true;
                        swappingCellY = i;
                        swapingCellX = j;
                    }
                    subMatrix[i, j] = matrix[Y + i, X + j];
                }
            }
            var table = visualizer.GetVisualization(subMatrix);
            if (swappingCellFound)
            {
                previewSwappingCellShow(table[swappingCellY, swapingCellX]);
            }
            return table;
        }
        public IEnumerable<T> GetNeighbours(Cell inputCell, Cell cellToExcludeFromNeighbours, bool includeDiagonalNeighbours)
        {
            int y = inputCell.Y, x = inputCell.X, excludeY = cellToExcludeFromNeighbours.Y, excludeX = cellToExcludeFromNeighbours.X;
            ToRealPosition(ref y, ref x);
            ToRealPosition(ref excludeY, ref excludeX);
            foreach (var neighbour in GetNeighbours(y, x, includeDiagonalNeighbours))
            {
                if (neighbour.Item1 == excludeY && neighbour.Item2 == excludeX)
                {
                    continue;
                }
                yield return matrix[neighbour.Item1, neighbour.Item2];
            }
        }
        private IEnumerable<Tuple<int, int>> GetNeighbours(int y, int x, bool includeDiagonalNeighbours)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i == 0 && j == 0)
                        || (!includeDiagonalNeighbours && i != 0 && j != 0)
                        || y + i < 0 || y + i == matrix.GetLength(0)
                        || x + j < 0 || x + j == matrix.GetLength(1)
                        || matrix[y + i, x + j] == null)
                    {
                        continue;
                    }
                    yield return new Tuple<int, int>(y + i, x + j);
                }
            }
        }
        public bool IsArticulationPoint(Cell cell, bool excludeSwappingCell, bool includeDiagonalNeighbours)
        {
            int y = cell.Y, x = cell.X;
            ToRealPosition(ref y, ref x);

            Queue<Tuple<int, int>> queue = new Queue<Tuple<int, int>>();

            int neighbourCount = 0;
            foreach (var neighbour in GetNeighbours(y, x, includeDiagonalNeighbours))
            {
                if (neighbour.Item1 == swapY && neighbour.Item2 == swapX)
                {
                    continue;
                }
                ++neighbourCount;
                if (neighbourCount == 1)
                {
                    queue.Enqueue(neighbour);
                }
                else
                {
                    break;
                }
            }
            if (neighbourCount == 1)
            {
                return false;
            }

            FieldState[,] fields = new FieldState[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    fields[i, j] = matrix[i, j] == null? FieldState.Empty : FieldState.NotVisited;
                }
            }
            fields[y, x] = FieldState.Visited;

            if (excludeSwappingCell)
            {
                fields[swapY, swapX] = FieldState.Empty;
            }

            while (queue.Count != 0)
            {
                var field = queue.Dequeue();
                fields[field.Item1, field.Item2] = FieldState.Visited;
                foreach (var neighbour in GetNeighbours(field.Item1, field.Item2, includeDiagonalNeighbours))
                {
                    if (fields[neighbour.Item1, neighbour.Item2] == FieldState.NotVisited)
                    {
                        fields[neighbour.Item1, neighbour.Item2] = FieldState.Processing;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            foreach (var field in fields)
            {
                if (field == FieldState.NotVisited)
                {
                    return true;
                }
            }
            return false;
        }
        private enum FieldState : byte
        {
            Visited, NotVisited, Processing, Empty
        }
    }
    public enum Direction : byte
    {
        Up, Down, Left, Right
    }
}
