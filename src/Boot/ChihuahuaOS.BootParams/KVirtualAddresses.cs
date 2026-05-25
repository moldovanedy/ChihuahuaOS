namespace ChihuahuaOS.BootParams;

public static class KVirtualAddresses
{
    // the kernel stack is 8 MiB

    public const ulong KERNEL_STACK_TOP = 0xFFFF_FFFF_FFFF_0000;

    public const ulong KERNEL_STACK_BOTTOM = 0xFFFF_FFFF_FF7F_0000;

    public const ulong KERNEL_STACK_OVERRUN_PROTECTOR = 0xFFFF_FFFF_FF7E_F000;


    // init-ramdisk has 1 GiB of space

    public const ulong INITRD_UPPER_LIMIT = 0xFFFF_FFFF_EFFF_FFFF;

    public const ulong INITRD_BASE = 0xFFFF_FFFF_B000_0000;


    // the GOP has 256 MiB

    public const ulong GOP_UPPER_LIMIT = 0xFFFF_EEEE_0FFF_FFFF;

    public const ulong GOP_BASE = 0xFFFF_EEEE_0000_0000;


    public const ulong KERNEL_INTENDED_START = 0xFFFF_8000_0000_0000;
}