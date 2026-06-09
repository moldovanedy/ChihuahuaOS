using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams.ParamsData;
using ChihuahuaOS.EfiApi.BootServices;

namespace ChihuahuaOS.BootParams;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct KParams
{
    public FbInfo FramebufferInfo;

    public VirtualAddressesInfo VirtualSpaceInfo;

    public KernelExecutableInfo KernelExecInfo;

    /// <summary>
    /// The EFI memory map itself.
    /// </summary>
    public EfiMemoryDescriptor* EfiMemMapStart;

    /// <summary>
    /// The number of entries in the EFI memory map.
    /// </summary>
    public ulong EfiMemMapNumEntries;

    /// <summary>
    /// The size of a single entry in the EFI memory map.
    /// </summary>
    public ulong EfiMemMapEntrySize;

    /// <summary>
    /// The entire size (in bytes) of the init-ramdisk.
    /// </summary>
    public ulong InitRdSize;

    /// <summary>
    /// The start of the 2 MiB free, identity-mapped memory chunk that the kernel can freely use.
    /// </summary>
    public ulong FreeMemChunkPhysicalAddress;
}
