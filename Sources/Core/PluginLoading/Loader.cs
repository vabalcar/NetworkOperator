using NetworkOperator.Core.CommunicationInterfaces;
using NetworkOperator.Core.OperationDescription;
using NetworkOperator.Core.UIMessanging;
using NetworkOperator.Core.Utils;
using System;
using System.IO;

namespace NetworkOperator.Core.PluginLoading
{
    internal class Loader
    {
        public const string OPERAND_SELECTORS_FOLDER = "OperandSelectors";
        public const string OPERATIONS_FOLDER = "Operations";
        public const string COMMUNICATION_INTERFACES_FOLDER = "CommunicationInterfaces";

        private NetworkOperator networkOperator;
        private PluginManager pluginManager;
        private ProgressCounter progressCounter = new ProgressCounter();
        public event Action<int> OnProgressChanged
        {
            add
            {
                progressCounter.OnProgressChanged += value;
            }
            remove
            {
                progressCounter.OnProgressChanged -= value;
            }
        }
        public Loader(NetworkOperator networkOperator)
        {
            this.networkOperator = networkOperator;
            pluginManager = new PluginManager(networkOperator);
        }
        public bool LoadAdditionalDlls()
        {
            Action onDllProcessed = () => progressCounter.IncreaseDoneWork(1);
            try
            {
                var operandSelectors = pluginManager.GetDlls(OPERAND_SELECTORS_FOLDER);
                var operations = pluginManager.GetDlls(OPERATIONS_FOLDER);
                var communicationInterfaces = pluginManager.GetDlls(COMMUNICATION_INTERFACES_FOLDER);

                progressCounter.Init(operandSelectors.Count + operations.Count + communicationInterfaces.Count);
                pluginManager.OnDllProcessed += onDllProcessed;

                pluginManager.RegisterChildren<OperandSelector>(operandSelectors);

                pluginManager.RegisterChildren<Operation>(operations, new object[] { networkOperator },
                    op => pluginManager.LoadRegisteredInstanceToField<Operation, OperandSelector>(op, op.OperandSelectorFieldName, op.OperandSelectorType));

                pluginManager.RegisterChildren<ICommunicationInterface>(communicationInterfaces);
                return true;
            }
            catch (IOException e)
            {
                UIUpdater.RaiseError(e.Message);
                return false;
            }
            finally
            {
                pluginManager.OnDllProcessed -= onDllProcessed;
            }
        }
    }
}
