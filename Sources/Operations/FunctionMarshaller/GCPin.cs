using System;
using System.Runtime.InteropServices;

namespace NetworkOperator.InteropServices
{
    public class GCPin<T> : IPin
    {
        private GCHandle GCHandle { get; set; }

        private T pinnedObject;
        public T PinnedObject
        {
            get
            {
                return pinnedObject;
            }
        }

        public IntPtr PointerToPinnedObject
        {
            get
            {
                return GetPointerToPinnedObject();
            }
        }

        protected virtual IntPtr GetPointerToPinnedObject() => GCHandle.AddrOfPinnedObject();

        public void Pin(T objectToPin)
        {
            pinnedObject = objectToPin;
            DoPin();
        }

        protected virtual void DoPin()
        {
            GC.SuppressFinalize(pinnedObject);
            GCHandle = GCHandle.Alloc(pinnedObject, GCHandleType.Pinned);
        }

        public virtual void Dispose()
        {
            if (pinnedObject == null)
            {
                return;
            }
            if (GCHandle.IsAllocated)
            {
                GCHandle.Free();
            }
            GC.ReRegisterForFinalize(PinnedObject);
        }
    }
}
