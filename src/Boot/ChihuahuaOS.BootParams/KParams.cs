using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams.ParamsData;
using ChihuahuaOS.EfiApi.BootServices;

namespace ChihuahuaOS.BootParams;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct KParams
{
    public FbInfo* FramebufferInfo;

    public EfiMemoryDescriptor* EfiMemMapStart;
    public ulong EfiMemMapNumEntries;
    public ulong EfiMemMapEntrySize;

    public ulong InitRdSize;
}