using System;
using System.Runtime.InteropServices;

namespace NetworkOperator.InteropServices
{
    public class DelegateGCPin<TDelegate> : GCPin<TDelegate>
    {
        bool marshaled = false;
        IntPtr pointerToDelegate;

        protected override IntPtr GetPointerToPinnedObject()
        {
            if (!marshaled)
            {
                pointerToDelegate = Marshal.GetFunctionPointerForDelegate(PinnedObject);
                marshaled = true;
            }
            return pointerToDelegate;
        }

        protected override void DoPin()
        {
            GC.SuppressFinalize(PinnedObject);
        }

        public override void Dispose()
        {
            if (PinnedObject == null)
            {
                return;
            }
            if (marshaled)
            {
                marshaled = false;
                Marshal.DestroyStructure(pointerToDelegate, pointerToDelegate.GetType());
            }
            base.Dispose();
        }
    }
}
