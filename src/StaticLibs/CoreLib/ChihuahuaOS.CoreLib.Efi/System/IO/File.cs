#if UEFI
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.SimpleFsProtocol;
using Internal.Runtime.CompilerHelpers;

namespace System.IO;

public unsafe class File
{
    public static EfiStatus LastOpenError { get; private set; } = EfiStatus.Success;

    /// <param name="path">Deviation from .NET: path is always absolute in this function.</param>
    /// <returns>
    /// Deviation from .NET: if something goes wrong, it returns null. Use <see cref="LastOpenError"/> to see what
    /// was the problem
    /// </returns>
    public static FileStream? OpenRead(string path)
    {
        return Open(path, FileMode.Open, FileAccess.Read);
    }

    /// <param name="path">Deviation from .NET: path is always absolute in this function.</param>
    /// <returns>
    /// Deviation from .NET: if something goes wrong, it returns null. Use <see cref="LastOpenError"/> to see what
    /// was the problem
    /// </returns>
    public static FileStream? OpenWrite(string path)
    {
        return Open(path, FileMode.OpenOrCreate, FileAccess.Write);
    }

    /// <param name="path">Deviation from .NET: path is always absolute in this function.</param>
    /// <param name="mode">
    /// Deviation from .NET: only <see cref="FileMode.Open"/> and <see cref="FileMode.OpenOrCreate"/> are supported.
    /// </param>
    /// <returns>
    /// Deviation from .NET: if something goes wrong, it returns null. Use <see cref="LastOpenError"/> to see what
    /// was the problem
    /// </returns>
    public static FileStream? Open(string path, FileMode mode)
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        return Open(path, mode, FileAccess.ReadWrite);
    }

    /// <param name="path">Deviation from .NET: path is always absolute in this function.</param>
    /// <param name="mode">
    /// Deviation from .NET: only <see cref="FileMode.Open"/> and <see cref="FileMode.OpenOrCreate"/> are supported.
    /// </param>
    /// <param name="access"></param>
    /// <returns>
    /// Deviation from .NET: if something goes wrong, it returns null. Use <see cref="LastOpenError"/> to see what
    /// was the problem
    /// </returns>
    public static FileStream? Open(string path, FileMode mode, FileAccess access)
    {
        if (mode != FileMode.Open && mode != FileMode.OpenOrCreate)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        EfiFileProtocol* newFile = null;
        var efiOpenMode = EfiFileOpenMode.Read;

        if (mode == FileMode.OpenOrCreate)
        {
            efiOpenMode = EfiFileOpenMode.Read | EfiFileOpenMode.Write | EfiFileOpenMode.Create;
        }
        else if (mode == FileMode.Create)
        {
            switch (access)
            {
                case FileAccess.Read:
                    efiOpenMode = EfiFileOpenMode.Read;
                    break;
                case FileAccess.Write:
                    efiOpenMode = EfiFileOpenMode.Write;
                    break;
                case FileAccess.ReadWrite:
                    efiOpenMode = EfiFileOpenMode.Read | EfiFileOpenMode.Write;
                    break;
            }
        }

        fixed (char* pathPtr = path)
        {
            LastOpenError = RootVolumeEfi.RawRootDir->Open(
                RootVolumeEfi.RawRootDir, &newFile, pathPtr, efiOpenMode, EfiFileAttributes.None);
            if (LastOpenError != EfiStatus.Success)
            {
                return null;
            }
        }

        if (newFile == null)
        {
            LastOpenError = EfiStatus.DeviceError;
        }

        return LastOpenError != EfiStatus.Success ? null : new FileStream(newFile);
    }
}

#endif