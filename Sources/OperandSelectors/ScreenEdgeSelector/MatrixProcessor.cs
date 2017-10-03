using System;

namespace NetworkOperator.OperandSelectors
{
    public static class MatrixProcessor
    {
        public static T[,] ExtendForView<T>(T[,] matrix, int members, int minHeight, int minWidth)
        {
            var extendedMatrix = new T[Math.Max(members, minHeight), Math.Max(members, minWidth)];
            int y = (extendedMatrix.GetLength(0) - matrix.GetLength(0)) / 2;
            int x = (extendedMatrix.GetLength(1) - matrix.GetLength(1)) / 2;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    extendedMatrix[y + i, x + j] = matrix[i, j];
                }
            }
            return extendedMatrix;
        }
        public static T[,] MinimizeMatrix<T>(T[,] matrix) where T : IEquatable<T>
        {
            var keepRows = new bool[matrix.GetLength(0)];
            var keepColumns = new bool[matrix.GetLength(1)];
            int rowsToKeep = 0, columnsToKeep = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i,j] != null && !matrix[i,j].Equals(default(T)))
                    {
                        if (keepRows[i] == false)
                        {
                            keepRows[i] = true;
                            ++rowsToKeep;
                        }
                        if (keepColumns[j] == false)
                        {
                            keepColumns[j] = true;
                            ++columnsToKeep;
                        }
                    }
                }
            }
            var minimizedMatrix = new T[rowsToKeep, columnsToKeep];
            int minimizedX = 0, minimizedY = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (keepRows[i] == false)
                {
                    continue;
                }
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (keepColumns[j] == false)
                    {
                        continue;
                    }
                    minimizedMatrix[minimizedY, minimizedX] = matrix[i, j];
                    ++minimizedX;
                }
                minimizedX = 0;
                ++minimizedY;
            }
            return minimizedMatrix;
        }
    }
}
