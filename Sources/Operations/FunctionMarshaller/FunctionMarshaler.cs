using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NetworkOperator.InteropServices
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void IntAction(int i);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StringAction([MarshalAs(UnmanagedType.LPStr), Out] string s);

    public class FunctionMarshaler : IDisposable
    {
        private Dictionary<IntPtr, IPin> pinnedDelegates = new Dictionary<IntPtr, IPin>();

        public IntPtr GetPointer<T>(T d)
        {
            var delegatePin = new DelegateGCPin<T>();
            delegatePin.Pin(d);
            pinnedDelegates.Add(delegatePin.PointerToPinnedObject, delegatePin);
            return delegatePin.PointerToPinnedObject;
        }
        public void DisposeDelegate(IntPtr delegatePtr)
        {
            if (pinnedDelegates.TryGetValue(delegatePtr, out IPin delegatePin))
            {
                delegatePin.Dispose();
                pinnedDelegates.Remove(delegatePtr);
            }
        }
        public void Dispose()
        {
            foreach (var pinnedDelegate in pinnedDelegates.Values)
            {
                pinnedDelegate.Dispose();
            }
        }
    }
}
