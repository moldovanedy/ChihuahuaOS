namespace System.Runtime.CompilerServices;

// A class responsible for running static constructors. The compiler will call into this
// code to ensure static constructors run and that they only run once.
internal static class ClassConstructorRunner
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Local

    [RuntimeExport("CheckStaticClassConstructionReturnNonGCStaticBase")]
    private static IntPtr CheckStaticClassConstructionReturnNonGCStaticBase(
        ref StaticClassConstructionContext context,
        IntPtr nonGcStaticBase)
    {
        CheckStaticClassConstruction(ref context);
        return nonGcStaticBase;
    }

    [RuntimeExport("CheckStaticClassConstructionReturnGCStaticBase")]
    private static IntPtr CheckStaticClassConstructionReturnGCStaticBase(
        ref StaticClassConstructionContext context,
        IntPtr gcStaticBase)
    {
        CheckStaticClassConstruction(ref context);
        return gcStaticBase;
    }

    // ReSharper restore UnusedMember.Local
    // ReSharper restore InconsistentNaming

    private static unsafe void CheckStaticClassConstruction(ref StaticClassConstructionContext context)
    {
        // Not dealing with multithreading issues in UEFI.
        if (context.initialized == IntPtr.Zero)
        {
            context.initialized = 1;
            if (context.cctorMethodAddress != IntPtr.Zero)
            {
                IntPtr address = context.cctorMethodAddress;
                ((delegate*<void>)address)();
            }
        }
    }
}