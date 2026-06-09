namespace ChihuahuaOS.EfiApi.BootServices;

public enum EfiMemoryType : uint
{
    EfiReservedMemoryType = 0,
    EfiLoaderCode = 1,
    EfiLoaderData = 2,
    EfiBootServicesCode = 3,
    EfiBootServicesData = 4,
    EfiRuntimeServicesCode = 5,
    EfiRuntimeServicesData = 6,
    EfiConventionalMemory = 7,
    EfiUnusableMemory = 8,
    EfiAcpiReclaimMemory = 9,
    EfiAcpiMemoryNvs = 10,
    EfiMemoryMappedIo = 11,
    EfiMemoryMappedIoPortSpace = 12,
    EfiPalCode = 13,
    EfiPersistentMemory = 14,
    EfiUnacceptedMemoryType = 15,
    EfiMaxMemoryType = 16,

    /// <summary>
    /// Memory for the kernel executable (.text, .data, .rodata, etc.). Mapped by the segment descriptors from the
    /// KParams.
    /// </summary>
    ChihuahuaKernelMemory = 0x80000000,

    /// <summary>
    /// Memory for init-ramdisk. Mapped to a randomized high address contained in KParams.
    /// </summary>
    ChihuahuaInitRdMemory = 0x80000001,

    /// <summary>
    /// Memory for the page tables themselves. Identity-mapped.
    /// </summary>
    ChihuahuaPageTables = 0x80000002,

    /// <summary>
    /// A 2 MiB continuous chunk of free memory for the kernel. Identity-mapped.
    /// </summary>
    ChihuahuaFreeKernelMemory = 0x80000003,

    /// <summary>
    /// Memory for the EFI memory map. Identity-mapped.
    /// </summary>
    ChihuahuaEfiMemMap = 0x80000004
}

public static class EfiMemoryTypeExtensions
{
    /// <summary>
    /// Returns true if the memory type is available for general use, false otherwise.
    /// </summary>
    /// <param name="memoryType"></param>
    /// <returns></returns>
    public static bool IsAvailable(this EfiMemoryType memoryType)
    {
        return memoryType == EfiMemoryType.EfiConventionalMemory
               || memoryType == EfiMemoryType.EfiBootServicesCode
               || memoryType == EfiMemoryType.EfiBootServicesData
               || memoryType == EfiMemoryType.EfiLoaderCode
               || memoryType == EfiMemoryType.EfiLoaderData;
    }
}
