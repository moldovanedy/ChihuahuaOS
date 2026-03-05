using System;
using System.Runtime.InteropServices;

namespace ChihuahuaOS.Bootloader.EfiApi.ConsoleSupport;

[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct EfiSimpleTextOutputProtocol
{
    /// <summary>
    /// Resets the entire console buffer. If the second parameter is true, the resetting will take more time and be
    /// more thorough.
    /// </summary>
    /// <returns>Success, DeviceError.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, bool, EfiStatus> Reset;

    /// <summary>
    /// Outputs the given string to the screen.
    /// </summary>
    /// <returns>Success, DeviceError, Unsupported, WarnUnknownGlyph</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, char*, EfiStatus> OutputString;

    /// <summary>
    /// Tests whether the given string can be written to the screen.
    /// </summary>
    /// <returns>Success, Unsupported</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, char*, EfiStatus> TestString;

    /// <summary>
    /// The second parameter is the mode number to query, the third is an out pointer for the number of columns,
    /// the fourth is an out pointer for the number of rows.
    /// </summary>
    /// <returns>Success, DeviceError, Unsupported.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, ulong, ulong*, ulong*, EfiStatus> QueryMode;

    /// <summary>
    /// The second parameter is the desired mode. On Success, it will clear the screen and reset the cursor pos.
    /// </summary>
    /// <returns>Success, DeviceError, Unsupported.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, ulong, EfiStatus> SetMode;

    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, EfiTextColor, EfiStatus> SetAttribute;

    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, EfiStatus> ClearScreen;

    /// <summary>
    /// The second parameter is the column position, the third is the row position.
    /// </summary>
    /// <returns>Success, DeviceError, Unsupported.</returns>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, ulong, ulong, EfiStatus> SetCursorPosition;

    /// <summary>
    /// If the second parameter is true, it will make the cursor visible, otherwise it will hide it.
    /// </summary>
    public readonly delegate* unmanaged<EfiSimpleTextOutputProtocol*, bool, EfiStatus> EnableCursor;

    public readonly SimpleTextOutputMode* Mode;
}

[Flags]
public enum EfiTextColor : ulong
{
    Black = 0x00,
    Blue = 0x01,
    Green = 0x02,
    Cyan = 0x03,
    Red = 0x04,
    Magenta = 0x05,
    Brown = 0x06,
    LightGray = 0x07,
    DarkGray = 0x08,
    LightBlue = 0x09,
    LightGreen = 0x0A,
    LightCyan = 0x0B,
    LightRed = 0x0C,
    LightMagenta = 0x0D,
    Yellow = 0x0E,
    White = 0x0F,

    BackgroundBlack = Black,
    BackgroundBlue = 0x10,
    BackgroundGreen = 0x20,
    BackgroundCyan = 0x30,
    BackgroundRed = 0x40,
    BackgroundMagenta = 0x50,
    BackgroundBrown = 0x60,
    BackgroundLightGray = 0x70
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct SimpleTextOutputMode
{
    public readonly int MaxMode;

    // current settings
    public readonly int Mode;
    public readonly int Attribute;
    public readonly int CursorColumn;
    public readonly int CursorRow;
    public readonly bool CursorVisible;
}