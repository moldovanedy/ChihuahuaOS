using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EfiGuid
{
    public readonly uint TimeLow;
    public readonly ushort TimeMid;
    public readonly ushort TimeHighAndVersion;
    public fixed byte ClockSeqAndNode[8];

    public EfiGuid(uint timeLow, ushort timeMid, ushort timeHighAndVersion, byte[] clockSeqAndNode)
    {
        TimeLow = timeLow;
        TimeMid = timeMid;
        TimeHighAndVersion = timeHighAndVersion;

        for (int i = 0; i < 8; i++)
        {
            ClockSeqAndNode[i] = clockSeqAndNode[i];
        }
    }
}