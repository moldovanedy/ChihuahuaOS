namespace ChihuahuaOS.EfiApi.BootServices;

public enum EfiMemoryType
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
    EfiMaxMemoryType = 16
}