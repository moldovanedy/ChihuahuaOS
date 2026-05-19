namespace ChihuahuaOS.BootParams;

public static class KVirtualAddresses
{
    public const ulong KERNEL_INTENDED_START = 0xFFFF_EEEE_8000_0000;

    public const ulong GOP_BASE = 0xFFFF_EEEE_0000_0000;

    public const ulong KERNEL_STACK_TOP = 0xFFFF_FFFF_FFFF_0000;

    public const ulong KERNEL_STACK_BOTTOM = 0xFFFF_FFFF_FF7F_0000;

    public const ulong KERNEL_STACK_OVERRUN_PROTECTOR = 0xFFFF_FFFF_FF7E_0000;
}