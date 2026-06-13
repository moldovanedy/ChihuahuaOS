using System.Runtime.InteropServices;

namespace ChihuahuaOS.BootParams.ParamsData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VirtualAddressesInfo
{
    public const ulong KERNEL_INTENDED_START = 0xFFFF_8000_0000_0000;

    public const ulong KERNEL_HIGHEST_POSSIBLE_STACK_TOP = 0xFFFF_FFFF_FFFF_0000;

    public const ulong KERNEL_STACK_SIZE = 256 * 1024;

    public ulong KStackTop;
    public ulong KStackBottom;

    public ulong InitRdLimit;
    public ulong InitRdBase;

    public ulong GopLimit;
    public ulong GopBase;
}
