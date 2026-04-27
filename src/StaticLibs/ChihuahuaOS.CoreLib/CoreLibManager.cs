namespace ChihuahuaOS.CoreLib;

/// <summary>
/// The essential functions for the standard library. The library will never modify these functions, just call them.
/// </summary>
public static unsafe class CoreLibManager
{
#if UEFI
    public static delegate* unmanaged<char*, void> Panic { get; set; }
#else
    public static delegate* unmanaged<byte*, void> Panic { get; set; }
#endif

    public static delegate* unmanaged<uint, void*> Malloc { get; set; }

    public static delegate* unmanaged<void*, void> Free { get; set; }
}