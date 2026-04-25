using System.Runtime.InteropServices;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiSimpleTextInputProtocol
{
    /// <summary>
    ///     Resets the entire input buffer. If the second parameter is true, the resetting will take more time and be
    ///     more thorough.
    /// </summary>
    /// <returns>Success, DeviceError.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextInputProtocol*, bool, EfiStatus> Reset;

    /// <summary>
    ///     Reads the next keystroke from the device; if there is no keystroke, this returns NotReady.
    /// </summary>
    /// <returns>Success, NotReady, DeviceError, Unsupported.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextInputProtocol*, EfiInputKey*, EfiStatus> ReadKeyStroke;

    /// <summary>
    ///     Event that waits for key input.
    /// </summary>
    public readonly EfiEvent WaitForKey;
}