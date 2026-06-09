using System.Runtime.InteropServices;

namespace ChihuahuaOS.BootParams.ParamsData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct KernelExecutableInfo
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SegmentDescriptor
    {
        public ulong PhysicalStart;
        public ulong VirtualStart;
        public ulong Size;
    }


    /// <summary>
    /// The number of segments that were loaded.
    /// </summary>
    public int NumSegmentsLoaded;

    /// <summary>
    /// The array of descriptors. It is of size <see cref="NumSegmentsLoaded"/>.
    /// </summary>
    public SegmentDescriptor* SegmentsDescriptorsArray;
}
