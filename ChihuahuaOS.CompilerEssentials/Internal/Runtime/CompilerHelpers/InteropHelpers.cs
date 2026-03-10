using System;
using System.Runtime.InteropServices;

namespace Internal.Runtime.CompilerHelpers;

public static unsafe class InteropHelpers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ModuleFixupCell
    {
        public IntPtr Handle;
        public IntPtr ModuleName;
        public IntPtr CallingAssemblyType;
        public uint DllImportSearchPathAndCookie;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MethodFixupCell
    {
        public IntPtr Target;
        public IntPtr MethodName;
        public ModuleFixupCell* Module;
        private int Flags;
    }


    // ReSharper disable once UnusedMember.Local
    private static IntPtr ResolvePInvoke(MethodFixupCell* pCell)
    {
        return pCell->Target != 0 ? pCell->Target : ResolvePInvokeSlow(pCell);
    }

    private static IntPtr ResolvePInvokeSlow(MethodFixupCell* pCell)
    {
        ModuleFixupCell* pModuleCell = pCell->Module;
        if (pModuleCell->Handle == 0)
        {
            ThrowHelpers.ThrowNullReferenceException();
        }

        if (pCell->Target == 0)
        {
            ThrowHelpers.ThrowNullReferenceException();
        }

        return pCell->Target;
    }
}