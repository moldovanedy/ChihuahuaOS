using System.Runtime.InteropServices;
using ChihuahuaOS.EfiApi.BootServices;
using ChihuahuaOS.EfiApi.ConsoleSupport;
using ChihuahuaOS.EfiApi.RuntimeServices;

namespace ChihuahuaOS.EfiApi.EfiSysTable;

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
    public readonly EfiRuntimeServices* RuntimeServices;
    public readonly EfiBootServices* BootServices;
    public readonly uint NumberOfTableEntries;
    public readonly void* ConfigurationTable;
}