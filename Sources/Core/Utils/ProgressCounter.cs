using System;

namespace NetworkOperator.Core.Utils
{
    internal class ProgressCounter
    {
        private Action<int> onProgressChanged;
        public event Action<int> OnProgressChanged
        {
            add
            {
                onProgressChanged += value;
            }
            remove
            {
                onProgressChanged -= value;
            }
        }
        private int wholeWorkSize;
        private int doneWorkSize;
        private int DoneWorkSize
        {
            get
            {
                return doneWorkSize;
            }
            set
            {
                doneWorkSize = value;
                onProgressChanged?.Invoke(Percentage);
            }
        }
        public int Percentage
        {
            get
            {
                return wholeWorkSize == 0 ? 0 : DoneWorkSize * 100 / wholeWorkSize;
            }
        }
        public void Reset()
        {
            DoneWorkSize = 0;
        }
        public void Init(int newWholeWorkSize)
        {
            Reset();
            wholeWorkSize = newWholeWorkSize;
        }
        public void Reset(int newWholeWorkSize) => Init(newWholeWorkSize);
        public void IncreaseDoneWork(int sizeOfNewlyDoneWork)
        {
            DoneWorkSize = DoneWorkSize + sizeOfNewlyDoneWork;
        }
    }
}
