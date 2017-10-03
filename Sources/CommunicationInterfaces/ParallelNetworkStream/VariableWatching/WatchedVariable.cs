namespace NetworkOperator.CommunicationInterfaces.VariableWatching
{
    public class WatchedVariable<T>
    {
        private T value;
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                watcher.Watch();
            }
        }
        private VariableWatcher watcher;
        public WatchedVariable(VariableWatcher watcher)
        {
            this.watcher = watcher;
        }
    }
}
