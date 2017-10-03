namespace NetworkOperator.CommunicationInterfaces.VariableWatching
{
    public class WatchedVariableFactory
    {
        private VariableWatcher watcher;

        public WatchedVariableFactory(VariableWatcher watcher)
        {
            this.watcher = watcher;
        }
        public WatchedVariable<T> Create<T>()
        {
            return new WatchedVariable<T>(watcher);
        }
        public WatchedVariable<T> Create<T>(T value)
        {
            return new WatchedVariable<T>(watcher) { Value = value };
        }
    }
}
