namespace NetworkOperator.CommunicationInterfaces.ParallelMultiTypeStreaming.Visitors
{
    public class StreamTypeNumberer : StreamTypeIdentifier<short>
    {
        private short id = 0;
        protected override short GenerateNewId() => id++;
    }
}
