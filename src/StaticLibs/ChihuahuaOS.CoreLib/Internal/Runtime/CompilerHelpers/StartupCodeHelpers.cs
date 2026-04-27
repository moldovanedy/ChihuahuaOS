using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using ChihuahuaOS.CoreLib;

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
        CoreLibManager.Panic("Fail fast called".ToCharPtrUnsafe());
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
            CoreLibManager.Panic("RhpNewArray Bad numElements".ToCharPtrUnsafe());
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
            CoreLibManager.Panic("RhpNewArrayFast Bad numElements".ToCharPtrUnsafe());
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
            CoreLibManager.Panic("RhpNewPtrArrayFast Bad numElements".ToCharPtrUnsafe());
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
            CoreLibManager.Panic("Assertion failed".ToCharPtrUnsafe()); /* covariance */
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

    [RuntimeExport("RhpByRefAssignRef")]
    internal static void RhpByRefAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

    [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
    internal static object? RhTypeCast_IsInstanceOfClass(MethodTable* pTargetMt, object? obj)
    {
        if (obj == null)
            return null;

        if (obj.m_pEEType == pTargetMt)
            return obj;

        // Simple implementation: check base type chain
        MethodTable* pMt = obj.m_pEEType;
        while (pMt != null)
        {
            if (pMt == pTargetMt)
                return obj;
            // In AOT, _relatedType often points to the parent for non-interface/non-array types
            pMt = pMt->_relatedType;
        }

        return null;
    }

    [RuntimeExport("RhpInitialDynamicInterfaceDispatch")]
    internal static void RhpInitialDynamicInterfaceDispatch()
    {
        CoreLibManager.Panic("RhpInitialDynamicInterfaceDispatch called".ToCharPtrUnsafe());
    }

    [RuntimeExport("__security_cookie")]
    internal static void* GetSecurityCookie()
    {
        return (void*)0x12345678;
    }

    [RuntimeExport("RhpWriteBarrier")]
    internal static void RhpWriteBarrier(void** dst, void* r)
    {
        *dst = r;
    }

    [RuntimeExport("RhpCheckedLockFreeAssignRef")]
    internal static void RhpCheckedLockFreeAssignRef(void** dst, void* r)
    {
        *dst = r;
    }

    private static MethodTable** AllocObject(uint size)
    {
        MethodTable** result = (MethodTable**)CoreLibManager.Malloc(size);

        // if (Environment.EfiSysTable == null)
        // {
        //     ThrowHelpers.ThrowNullReferenceException();
        //     return null;
        // }
        //
        // EfiStatus status =
        //     Environment.EfiSysTable->BootServices->AllocatePool(EfiMemoryType.EfiLoaderData, size, (void**)&result);
        // if (status != EfiStatus.Success)
        // {
        //     result = null;
        // }
        //
        // if (result == null)
        // {
        //     Environment.FailFast("Allocation failed");
        // }

        //zero out memory
        SpanHelpers.Fill(ref *(byte*)result, 0, size);
        return result;
    }

    internal struct ArrayElement
    {
        public object Value;
    }
}