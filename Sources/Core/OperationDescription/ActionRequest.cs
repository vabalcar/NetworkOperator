namespace NetworkOperator.Core.OperationDescription
{
    public class ActionRequest
    {
        public short ParentOperation { get; set; }

        public byte[] Content { get; set; }

        public ActionRequest()
        {
        }
    }
}
