using System.Runtime.InteropServices;

namespace NetworkOperator.Core.OperationDescription
{
    public abstract class OperationComponent
    {
        protected Operation parent;

        private bool running = false;
        private bool alive = false;

        protected NetworkOperator networkOperator;

        public OperationComponent(NetworkOperator networkOperator, Operation parent)
        {
            this.networkOperator = networkOperator;
            this.parent = parent;
        }

        internal void Start()
        {
            if (!running && !alive)
            {
                alive = true;
                running = true;
                StartAction();
            }
        }
        public abstract void StartAction();
        internal void Pause()
        {
            if (running && alive)
            {
                running = false;
                PauseAction();
            }
        }
        public abstract void PauseAction();
        internal void Resume()
        {
            if (!running && alive)
            {
                running = true;
                ResumeAction();
            }
        }
        internal void Stop()
        {
            if (running && alive)
            {
                running = false;
                alive = false;
                StopAction();
            }
        }
        public abstract void StopAction();
        public abstract void ResumeAction();
    }
}
