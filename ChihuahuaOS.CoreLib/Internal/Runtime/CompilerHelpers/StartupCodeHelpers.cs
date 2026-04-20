using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.BootServices;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// Contains all the internals for object memory allocation and GC.
/// </summary>
internal unsafe class StartupCodeHelpers
{
    // A couple symbols the generated code will need we park them in this class
    // for no particular reason. These aid in transitioning to/from managed code.
    // Since we don't have a GC, the transition is a no-op.
    [RuntimeExport("RhpReversePInvoke")]
    internal static void RhpReversePInvoke(IntPtr frame)
    {
    }

    [RuntimeExport("RhpReversePInvokeReturn")]
    internal static void RhpReversePInvokeReturn(IntPtr frame)
    {
    }

    [RuntimeExport("RhpPInvoke")]
    internal static void RhpPInvoke(IntPtr frame)
    {
    }

    [RuntimeExport("RhpPInvokeReturn")]
    internal static void RhpPInvokeReturn(IntPtr frame)
    {
    }

    [RuntimeExport("RhpGcPoll")]
    internal static void RhpGcPoll()
    {
    }

    [RuntimeExport("RhpFallbackFailFast")]
    internal static void RhpFallbackFailFast()
    {
        Environment.FailFast("Fail fast called");
    }

    [RuntimeExport("RhpNewFast")]
    internal static void* RhpNewFast(MethodTable* pMt)
    {
        MethodTable** result = AllocObject(pMt->_uBaseSize);
        *result = pMt;
        return result;
    }

    [RuntimeExport("RhpNewArray")]
    internal static void* RhpNewArray(MethodTable* pMt, int numElements)
    {
        if (numElements < 0)
        {
            Environment.FailFast("RhpNewArray Bad numElements");
        }

        MethodTable** result = AllocObject((uint)(pMt->_uBaseSize + numElements * pMt->_usComponentSize));
        *result = pMt;
        *(int*)(result + 1) = numElements;
        return result;
    }

    [RuntimeExport("RhpNewArrayFast")]
    internal static void* RhpNewArrayFast(MethodTable* pMt, int numElements)
    {
        if (numElements < 0)
        {
            Environment.FailFast("RhpNewArrayFast Bad numElements");
        }

        MethodTable** result = AllocObject((uint)(pMt->_uBaseSize + numElements * pMt->_usComponentSize));
        *result = pMt;
        *(int*)(result + 1) = numElements;
        return result;
    }

    [RuntimeExport("RhpNewPtrArrayFast")]
    internal static void* RhpNewPtrArrayFast(MethodTable* pMt, int numElements)
    {
        if (numElements < 0)
        {
            Environment.FailFast("RhpNewPtrArrayFast Bad numElements");
        }

        MethodTable** result = AllocObject((uint)(pMt->_uBaseSize + numElements * pMt->_usComponentSize));
        *result = pMt;
        *(int*)(result + 1) = numElements;
        return result;
    }

    [RuntimeExport("RhpStelemRef")]
    internal static void StelemRef(Array array, nint index, object? obj)
    {
        ref object element = ref Unsafe.As<ArrayElement[]>(array)[index].Value;
        MethodTable* elementType = array.m_pEEType->_relatedType;

        if (obj == null)
        {
            element = null!;
            return;
        }

        if (elementType != obj.m_pEEType)
        {
            Environment.FailFast("Assertion failed"); /* covariance */
        }

        element = obj;
    }

    [RuntimeExport("RhpCheckedAssignRef")]
    internal static void RhpCheckedAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

    [RuntimeExport("RhpAssignRef")]
    internal static void RhpAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

    private static MethodTable** AllocObject(uint size)
    {
        MethodTable** result;

#if UEFI || DEBUG

        if (Environment.EfiSysTable == null)
        {
            ThrowHelpers.ThrowNullReferenceException();
            return null;
        }

        EfiStatus status =
            Environment.EfiSysTable->BootServices->AllocatePool(EfiMemoryType.EfiLoaderData, size, (void**)&result);
        if (status != EfiStatus.Success)
        {
            result = null;
        }
#endif

        if (result == null)
        {
            Environment.FailFast("Allocation failed");
        }

        return result;
    }

    internal struct ArrayElement
    {
        public object Value;
    }
}