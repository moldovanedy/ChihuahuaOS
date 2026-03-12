using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public readonly struct EfiGuid
{
    public readonly ulong Low;
    public readonly ulong High;

    public EfiGuid(ulong low, ulong high)
    {
        Low = low;
        High = high;
    }
}