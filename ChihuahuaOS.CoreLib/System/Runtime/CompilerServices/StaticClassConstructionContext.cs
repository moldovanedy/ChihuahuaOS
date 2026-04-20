using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Sequential)]
public struct StaticClassConstructionContext
{
    //this is most likely a contract with the runtime; we don't rename it

    // ReSharper disable once InconsistentNaming
    public volatile IntPtr cctorMethodAddress;

    public volatile IntPtr initialized;
}