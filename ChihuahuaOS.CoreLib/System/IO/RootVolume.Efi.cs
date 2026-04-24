using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.SimpleFsProtocol;

#if UEFI || DEBUG

namespace System.IO;

internal static unsafe class RootVolumeEfi
{
    public static EfiStatus LastError { get; private set; } = EfiStatus.Success;

    public static EfiFileProtocol* RawRootDir
    {
        get
        {
            if (field == null)
            {
                EfiGuid simpleFsGuid = AllEfiGuids.EfiSimpleFs;
                EfiSimpleFsProtocol* fsProtocol = null;
                LastError = Environment.EfiSysTable->BootServices->LocateProtocol(
                    &simpleFsGuid,
                    null,
                    (void**)&fsProtocol);

                EfiFileProtocol* rootDir = null;
                fsProtocol->OpenVolume(fsProtocol, &rootDir);
                field = rootDir;
            }

            return field;
        }
    }
}

#endif