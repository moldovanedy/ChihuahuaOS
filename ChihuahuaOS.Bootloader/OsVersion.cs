using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader;

[StructLayout(LayoutKind.Sequential)]
public struct OsVersion
{
    public byte MajorVersion;
    public byte MinorVersion;
    public byte Patch;
    public byte Revision;

    public uint BuildNumber;


    public OsVersion(byte majorVersion, byte minorVersion, byte patch, byte revision = 0, uint buildNumber = 0)
    {
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        Patch = patch;
        Revision = revision;
        BuildNumber = buildNumber;
    }

    public override string ToString()
    {
        using string majorVer = MajorVersion.ToString();
        using string minorVer = MinorVersion.ToString();
        using string patch = Patch.ToString();
        using string revision = Revision.ToString();
        using string buildNumber = BuildNumber.ToString();

        string mainString = majorVer + "." + minorVer + "." + patch;
        if (Revision != 0)
        {
            using string prevString = mainString;
            using string tempString = "." + revision;
            mainString = prevString + tempString;
        }

        if (BuildNumber != 0)
        {
            using string prevString = mainString;
            using string tempString = "." + buildNumber;
            mainString = prevString + tempString;
        }

        return mainString;
    }
}