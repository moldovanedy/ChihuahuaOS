using System;

namespace Internal.Runtime.CompilerHelpers;

/// <summary>
/// These methods are used to implement ldtoken instruction.
/// </summary>
internal static class LdTokenHelpers
{
    internal static unsafe RuntimeMethodHandle GetRuntimeMethodHandle(IntPtr pHandleSignature)
    {
        RuntimeMethodHandle returnValue;
        *(IntPtr*)&returnValue = pHandleSignature;
        return returnValue;
    }

    internal static unsafe RuntimeFieldHandle GetRuntimeFieldHandle(IntPtr pHandleSignature)
    {
        RuntimeFieldHandle returnValue;
        *(IntPtr*)&returnValue = pHandleSignature;
        return returnValue;
    }
}