using NetworkOperator.Core.OperationDescription;
using NetworkOperator.Core.CommunicationInterfaces;
using System.Collections.Generic;
using NetworkOperator.Core;

namespace NetworkOperator.UserInterfaces
{
    public class OperationAutoStartManager
    {
        private List<string> autostartedOperations;
        private List<string> AutostartedOperations
        {
            get
            {
                if (autostartedOperations == null && ConfigurationManager.Current.IsConfigurationAvailable())
                {
                    autostartedOperations = ConfigurationManager.Current.LoadList<string>();
                }
                else if (autostartedOperations == null)
                {
                    autostartedOperations = new List<string>();
                }
                return autostartedOperations;
            }
        }

        private static OperationAutoStartManager current;
        public static OperationAutoStartManager Current
        {
            get
            {
                if (current == null)
                {
                    current = new OperationAutoStartManager();
                }
                return current;
            }
        }

        private OperationAutoStartManager()
        {
        }
        public void SetAutostartProperty(Operation operation, bool autostart)
        {
            string operationName = operation.GetType().FullName;
            if (autostart && !AutostartedOperations.Contains(operationName))
            {
                AutostartedOperations.Add(operationName);
            }
            else if (AutostartedOperations.Contains(operationName))
            {
                AutostartedOperations.Remove(operationName);
            }
        }
        public void Autostart(IReadOnlyRegisterAccessor<Operation> operations)
        {
            foreach (var operation in AutostartedOperations)
            {
                operations[operation].PerformAction(OperationAction.Start);
            }
        }
        public bool IsAutostartedOperation(Operation operation)
            => AutostartedOperations.Contains(operation.GetType().FullName);

        ~OperationAutoStartManager()
        {
            if (autostartedOperations != null)
            {
                ConfigurationManager.Current.StoreConfiguration(autostartedOperations);
            }
        }
    }
}
