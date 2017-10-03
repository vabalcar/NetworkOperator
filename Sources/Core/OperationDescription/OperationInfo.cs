namespace NetworkOperator.Core.OperationDescription
{
    public class OperationInfo
    {
        public Operation Operation { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public OperationInfo(Operation parentOperation)
        {
            Operation = parentOperation;
        }
    }
}
