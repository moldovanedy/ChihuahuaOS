namespace ChihuahuaOS.EfiApi.RuntimeServices;

public enum EfiResetType
{
    /// <summary>
    /// Cold reset (hard restart).
    /// </summary>
    EfiResetCold,

    /// <summary>
    /// Warm reset (some sort of soft restart). If not supported, will do a <see cref="EfiResetCold"/> instead.
    /// </summary>
    EfiResetWarm,

    /// <summary>
    /// Shutdown (power off).
    /// </summary>
    EfiResetShutdown,

    EfiResetPlatformSpecific
}