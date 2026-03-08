using System;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace Internal.Runtime.CompilerHelpers;

// A class that the compiler looks for that has helpers to initialize the
// process. The compiler can gracefully handle the helpers not being present,
// but the class itself being absent is unhandled. Let's add an empty class.
public unsafe class StartupCodeHelpers
{
    // A couple symbols the generated code will need we park them in this class
    // for no particular reason. These aid in transitioning to/from managed code.
    // Since we don't have a GC, the transition is a no-op.
    [RuntimeExport("RhpReversePInvoke")]
    private static void RhpReversePInvoke(IntPtr frame)
    {
    }

    [RuntimeExport("RhpReversePInvokeReturn")]
    private static void RhpReversePInvokeReturn(IntPtr frame)
    {
    }

    [RuntimeExport("RhpPInvoke")]
    private static void RhpPInvoke(IntPtr frame)
    {
    }

    [RuntimeExport("RhpPInvokeReturn")]
    private static void RhpPInvokeReturn(IntPtr frame)
    {
    }

    [RuntimeExport("RhpGcPoll")]
    private static void RhpGcPoll()
    {
    }

    [RuntimeExport("RhpFallbackFailFast")]
    private static void RhpFallbackFailFast()
    {
        Environment.FailFast("Fail fast called");
    }

    [RuntimeExport("RhpNewFast")]
    private static unsafe void* RhpNewFast(MethodTable* pMT)
    {
        // MethodTable** result = AllocObject(pMT->_uBaseSize);
        // *result = pMT;
        // return result;

        return (void*)0;
    }

    [RuntimeExport("RhpNewArray")]
    private static unsafe void* RhpNewArray(MethodTable* pMT, int numElements)
    {
        // if (numElements < 0)
        //     Environment.FailFast(null);
        //
        // MethodTable** result = AllocObject((uint)(pMT->_uBaseSize + numElements * pMT->_usComponentSize));
        // *result = pMT;
        // *(int*)(result + 1) = numElements;
        // return result;

        return (void*)0;
    }

    internal struct ArrayElement
    {
        public object Value;
    }

    [RuntimeExport("RhpStelemRef")]
    public static void StelemRef(Array array, nint index, object? obj)
    {
        ref object element = ref Unsafe.As<ArrayElement[]>(array)[index].Value;
        MethodTable* elementType = array.m_pMethodTable->_relatedType;

        if (obj == null)
        {
            element = null!;
            return;
        }

        if (elementType != obj.m_pMethodTable)
        {
            Environment.FailFast("Assertion failed"); /* covariance */
        }

        element = obj;
    }

    [RuntimeExport("RhpCheckedAssignRef")]
    public static unsafe void RhpCheckedAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

    [RuntimeExport("RhpAssignRef")]
    public static unsafe void RhpAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

//         static unsafe MethodTable** AllocObject(uint size)
//         {
// #if WINDOWS
//             [DllImport("kernel32"), SuppressGCTransition]
//             static extern MethodTable** LocalAlloc(uint flags, uint size);
//             MethodTable** result = LocalAlloc(0x40, size);
// #elif LINUX
//             [DllImport("libSystem.Native"), SuppressGCTransition]
//             static extern MethodTable** SystemNative_Malloc(nuint size);
//             MethodTable** result = SystemNative_Malloc(size);
// #elif UEFI
//             MethodTable** result;
//             if (EfiSystemTable->BootServices->AllocatePool(2 /* LoaderData*/, (nint)size, (void**)&result) != 0)
//                 result = null;
// #else
// #error Nope
// #endif
//
//             if (result == null)
//                 Environment.FailFast(null);
//
//             return result;
//         }
}