using NetworkOperator.Core;
using NetworkOperator.IO;
using System;
using System.Collections.Generic;
using System.Net;
using NetworkOperator.Core.OperationDescription;
using System.Linq;

namespace NetworkOperator.OperandSelectors
{
    public class ScreenEdgeSelector : OperandSelector<ScreenEdge>
    {
        private Action<string> onOperandChanged;
        public override event Action<string> OnOperandChanged
        {
            add => onOperandChanged += value;
            remove => onOperandChanged -= value;
        }

        public int OperandCount { get; private set; }
        private string selectedOperand;
        public override string SelectedOperand
        {
            get => selectedOperand;
            protected set
            {
                IsLocalMachineSelected = value == LocalMachineName;
                selectedOperand = value;
                onOperandChanged?.Invoke(selectedOperand);
            }
        }
        private int[] currentPosition = new int[2];

        private string[,] operandMatrix;
        public string[,] OperandMatrix
        {
            get
            {
                if (operandMatrix == null)
                {
                    SetOperandsAvailability(null, 0);
                }
                var copy = new string[operandMatrix.GetLength(0), operandMatrix.GetLength(1)];
                for (int i = 0; i < copy.GetLength(0); i++)
                {
                    for (int j = 0; j < copy.GetLength(1); j++)
                    {
                        copy[i, j] = operandMatrix[i, j];
                    }
                }
                return copy;
            }
            set
            {
                if (value != null)
                {
                    operandMatrix = value;
                    for (int i = 0; i < operandMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < operandMatrix.GetLength(1); j++)
                        {
                            if (operandMatrix[i, j] != null && operandMatrix[i, j].Length == 0)
                            {
                                operandMatrix[i, j] = null;
                            }
                        }
                    }
                    if (IsOperandAvailable(selectedOperand))
                    {
                        SelectOperand(selectedOperand);
                    }
                    else
                    {
                        DoSelectOperand(LocalMachineName);
                    }
                }
                else
                {
                    throw new ArgumentNullException($"Value of type {nameof(OperandMatrix)} is null");
                }
            }
        }

        private Dictionary<string, bool> operandsAvailability = new Dictionary<string, bool>();

        public override string LocalMachineName { get; protected set; } = Dns.GetHostName();//TODO: check this out
        public override bool IsLocalMachineSelected { get; protected set; } = true;
        public override bool AutoInformOthersAboutNewOperand { get; protected set; } = true;
        public override bool IsReliableTransferRequired { get; protected set; } = false;

        public Configuration Configuration
        {
            get => new Configuration()
            {
                Description = "Screen positions settings",
                ConfigurationImplementations = new List<object>() { new ScreenEdgeSelectorConfiguratorWindow(this) }
            };
        }

        private object internalLock = new object();
        public ScreenEdgeSelector()
        {
            selectedOperand = LocalMachineName;
            if (ConfigurationManager.Current.IsConfigurationAvailable())
            {
                OperandMatrix = ConfigurationManager.Current.LoadMatrix<string>();
                foreach (var operand in operandMatrix)
                {
                    operandsAvailability.Add(operand, false);
                }
            }
            RegisterOperand(LocalMachineName);
        }
        public override void SetOperandsAvailability(IEnumerable<string> availableOperands, int availableOperandsCount)
        {
            lock(internalLock)
            {
                if (availableOperands != null)
                {
                    foreach (var availableOperand in availableOperands)
                    {
                        RegisterOperand(availableOperand);
                    }
                }
            }
        }
        public override void RegisterOperand(string operand)
        {
            lock(internalLock)
            {
                DoRegisterOperand(operand);
            }
        }
        private void DoRegisterOperand(string operand)
        {
            if (operandsAvailability.TryGetValue(operand, out bool value))
            {
                operandsAvailability[operand] = true;
            }
            else
            {
                AddOperand(operand);
                operandsAvailability.Add(operand, true);
            }
            ++OperandCount;
        }
        private void AddOperand(string operand)
        {
            if (operandMatrix == null)
            {
                operandMatrix = new string[1, 1];
                operandMatrix[0, 0] = operand;
                return;
            }
            if (operandMatrix.Length == OperandCount)
            {
                var newMatrix = new string[operandMatrix.GetLength(0), operandMatrix.GetLength(1) + 1];
                for (int i = 0; i < operandMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < operandMatrix.GetLength(1); j++)
                    {
                        newMatrix[i, j] = operandMatrix[i, j];
                    }
                }
                OperandMatrix = newMatrix;
            }
            for (int i = 0; i < operandMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < operandMatrix.GetLength(1); j++)
                {
                    if (operandMatrix[i,j] != null)
                    {
                        continue;
                    }
                    if (j != 0 && operandMatrix[j - 1, i] != null)
                    {
                        operandMatrix[i, j] = operand;
                        return;
                    }
                    if (j != operandMatrix.GetLength(1) - 1 && operandMatrix[i, j + 1] != null)
                    {
                        operandMatrix[i, j] = operand;
                        return;
                    }
                    if (i != 0 && operandMatrix[i - 1, j] != null)
                    {
                        operandMatrix[i, j] = operand;
                        return;
                    }
                    if (i != operandMatrix.GetLength(0) - 1 && operandMatrix[i + 1, j] != null)
                    {
                        operandMatrix[i, j] = operand;
                        return;
                    }
                }
            }
        }
        public override void UnregisterOperand(string operand)
        {
            if (operandsAvailability.TryGetValue(operand, out bool value))
            {
                operandsAvailability[operand] = false;
                if (operand == selectedOperand)
                {
                    SelectOperand(LocalMachineName);
                }
            }
            --OperandCount;
        }
        public override void UnregisterAllOperands()
        {
            operandsAvailability = operandsAvailability.ToDictionary(pair => pair.Key, operand => false);
            RegisterOperand(LocalMachineName);
        }
        public override bool IsOperandAvailable(string operand) =>
            operandsAvailability != null && operandsAvailability.TryGetValue(operand, out bool value) && value;
        public override void SelectOperand(string operand)
        {
            if (operandsAvailability.TryGetValue(operand, out bool value) && value)
            {
                DoSelectOperand(operand);
            }
        }
        private void DoSelectOperand(string operand)
        {
            int fieldCount = -1;
            foreach (var field in operandMatrix)
            {
                ++fieldCount;
                if (field == operand)
                {
                    currentPosition[0] = fieldCount / operandMatrix.GetLength(1);
                    currentPosition[1] = fieldCount % operandMatrix.GetLength(1);
                    break;
                }
            }
            SelectedOperand = operand;
        }
        public override void SelectOperand(ScreenEdge direction, Action<string> onSuccess)
        {
            if (TrySelectOperand(direction))
            {
                onSuccess?.Invoke(selectedOperand);
            }
        }
        private bool TrySelectOperand(ScreenEdge direction)
        {
            int[] position;
            position = new int[] { currentPosition[0], currentPosition[1] };
            while (true)
            {
                switch (direction)
                {
                    case ScreenEdge.Top:
                        --position[0];
                        break;
                    case ScreenEdge.Bottom:
                        ++position[0];
                        break;
                    case ScreenEdge.Left:
                        --position[1];
                        break;
                    case ScreenEdge.Right:
                        ++position[1];
                        break;
                    default:
                        return false;
                }
                if (!(position[0] >= 0 && position[1] >= 0
                    && position[0] < operandMatrix.GetLength(0) && position[1] < operandMatrix.GetLength(1)))
                {
                    break;
                }
                if (IsOperandAvailable(operandMatrix[position[0], position[1]]))
                {
                    SelectedOperand = operandMatrix[position[0], position[1]];
                    currentPosition = position;
                    return true;
                }
            }
            return false;
        }
        public void StoreOperandMatrix() => ConfigurationManager.Current.StoreConfiguration(operandMatrix);
        ~ScreenEdgeSelector()
        {
            StoreOperandMatrix();
        }
    }
}
