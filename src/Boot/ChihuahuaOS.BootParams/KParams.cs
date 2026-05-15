using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams.ParamsData;

namespace ChihuahuaOS.BootParams;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct KParams
{
    public FbInfo* FramebufferInfo;
}