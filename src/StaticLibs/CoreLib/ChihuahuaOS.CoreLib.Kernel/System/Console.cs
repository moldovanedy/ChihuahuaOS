namespace System;

public static unsafe class Console
{
    private static delegate* unmanaged<ConsoleColor, void> _changeBgColor;
    private static delegate* unmanaged<ConsoleColor, void> _changeFgColor;

    private static delegate* unmanaged<(uint, uint)> _requestBufferSize;
    private static delegate* unmanaged<(uint, uint)> _requestCursorPos;
    private static delegate* unmanaged<uint, uint, void> _setCursorPos;

    private static delegate* unmanaged<bool, void> _setCursorVisibility;
    private static delegate* unmanaged<byte*, void> _print;
    private static delegate* unmanaged<void> _fastClear;

    public static void SetupHandlers(
        delegate* unmanaged<ConsoleColor, void> changeBgColor,
        delegate* unmanaged<ConsoleColor, void> changeFgColor,
        delegate* unmanaged<(uint, uint)> requestBufferSize,
        delegate* unmanaged<(uint, uint)> requestCursorPos,
        delegate* unmanaged<uint, uint, void> setCursorPos,
        delegate* unmanaged<bool, void> setCursorVisibility,
        delegate* unmanaged<byte*, void> print,
        delegate* unmanaged<void> fastClear)
    {
        _changeBgColor = changeBgColor;
        _changeFgColor = changeFgColor;

        _requestBufferSize = requestBufferSize;
        _requestCursorPos = requestCursorPos;
        _setCursorPos = setCursorPos;

        _setCursorVisibility = setCursorVisibility;
        _print = print;
        _fastClear = fastClear;
    }

    #region Properties

    public static ConsoleColor BackgroundColor
    {
        get;
        set
        {
            field = value;
            _changeBgColor(value);
        }
    } = ConsoleColor.Black;

    public static int BufferHeight => (int)_requestBufferSize().Item2;

    public static int BufferWidth => (int)_requestBufferSize().Item1;

    public static int CursorLeft
    {
        get => (int)_requestCursorPos().Item1;
        set => _setCursorPos((uint)value, (uint)CursorTop);
    }

    public static int CursorTop
    {
        get => (int)_requestCursorPos().Item2;
        set => _setCursorPos((uint)CursorLeft, (uint)value);
    }

    public static bool CursorVisible
    {
        get;
        set
        {
            field = value;
            _setCursorVisibility(value);
        }
    } = false;

    public static ConsoleColor ForegroundColor
    {
        get;
        set
        {
            field = value;
            _changeFgColor(value);
        }
    } = ConsoleColor.White;

    #endregion

    #region Methods

    public static void Clear()
    {
        _setCursorPos(0, 0);
        _fastClear();
    }

    public static void WriteLine(ReadOnlySpan<byte> text)
    {
        _print((byte*)text);
        _print((byte*)"\n\0"u8);
    }

    /// <summary>
    /// Extension
    /// </summary>
    /// <param name="text"></param>
    public static void WriteLineRaw(byte* text)
    {
        _print(text);
        _print((byte*)"\n\0"u8);
    }

    public static void Write(ReadOnlySpan<byte> text)
    {
        _print((byte*)text);
    }

    /// <summary>
    /// Extension
    /// </summary>
    /// <param name="text"></param>
    public static void WriteRaw(byte* text)
    {
        _print(text);
    }

    #endregion
}
