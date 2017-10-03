using NetworkOperator.Core.OperationDescription;

namespace NetworkOperator.Core.CommunicationInterfaces
{
    public interface IUserInterfaceController
    {
        SessionMode SessionMode { get; set; }
        IReadOnlyRegisterAccessor<Operation> Operations { get; }
        void Load();
        void End();
    }
    public enum SessionMode : byte
    {
        Normal, Debug
    }
}
