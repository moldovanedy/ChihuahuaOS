using System.Runtime.InteropServices;
using ChihuahuaOS.Bootloader.EfiApi.BootServices;
using ChihuahuaOS.Bootloader.EfiApi.ConsoleSupport;

namespace ChihuahuaOS.Bootloader.EfiApi.EfiSysTable;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiSystemTable
{
    public readonly EfiTableHeader Hdr;
    public readonly char* FirmwareVendor;
    public readonly uint FirmwareRevision;
    public readonly EfiHandle ConsoleInHandle;
    public readonly EfiSimpleTextInputProtocol* ConIn;
    public readonly EfiHandle ConsoleOutHandle;
    public readonly EfiSimpleTextOutputProtocol* ConOut;
    public readonly EfiHandle StandardErrorHandle;
    public readonly void* StdErr;
    public readonly void* RuntimeServices;
    public readonly EfiBootServices* BootServices;
    public readonly uint NumberOfTableEntries;
    public readonly void* ConfigurationTable;
}