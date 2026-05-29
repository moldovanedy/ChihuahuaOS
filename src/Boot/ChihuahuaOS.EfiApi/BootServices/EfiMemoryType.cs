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

    ChihuahuaKernelMemory = 0x80000000,
    ChihuahuaInitRdMemory = 0x80000001,
    ChihuahuaPageTables = 0x80000002,
    ChihuahuaFreeKernelMemory = 0x80000003
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
