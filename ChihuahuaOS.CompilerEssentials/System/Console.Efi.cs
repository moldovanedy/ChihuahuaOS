#if UEFI || DEBUG

using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.ConsoleSupport;
using ChihuahuaOS.EfiApi.EfiSysTable;

namespace System;

public static unsafe partial class Console
{
    private const string NEWLINE = "\r\n\0";

    private static EfiStatus _lastStatus = EfiStatus.Success;

    private static EfiTextColor _overallEfiTextColor = EfiTextColor.White;

    private static (int Cols, int Rows) PrivateGetBufferSize()
    {
        EfiSystemTable* st = Environment.EfiSysTable;

        ulong row = 0;
        ulong col = 0;
        EfiStatus status = st->ConOut->QueryMode(st->ConOut, (ulong)st->ConOut->Mode->Mode, &col, &row);
        return
            status != EfiStatus.Success
                ? (80, 25)
                : (col != 0 ? (int)col : 80, row != 0 ? (int)row : 25);
    }

    #region Properties

    /// <summary>
    /// Sets the background color. Only the first 7 colors are accepted for BG, otherwise this property will
    /// not change.
    /// </summary>
    public static ConsoleColor BackgroundColor
    {
        get;
        set
        {
            if ((int)value > 7)
            {
                return;
            }

            field = value;
            _overallEfiTextColor = (EfiTextColor)ForegroundColor | (EfiTextColor)((int)value * 0x10);

            EfiSystemTable* st = Environment.EfiSysTable;
            if (st == null)
            {
                return;
            }

            st->ConOut->SetAttribute(st->ConOut, _overallEfiTextColor);
        }
    } = ConsoleColor.Black;

    public static int BufferHeight => Environment.EfiSysTable == null ? 0 : PrivateGetBufferSize().Rows;

    public static int BufferWidth => Environment.EfiSysTable == null ? 0 : PrivateGetBufferSize().Cols;

    public static int CursorLeft
    {
        get;
        set
        {
            EfiSystemTable* st = Environment.EfiSysTable;
            field = value;
            if (st == null)
            {
                return;
            }

            st->ConOut->SetCursorPosition(st->ConOut, (ulong)value, (ulong)CursorTop);
        }
    } = 0;

    public static int CursorTop
    {
        get;
        set
        {
            EfiSystemTable* st = Environment.EfiSysTable;
            field = value;
            if (st == null)
            {
                return;
            }

            st->ConOut->SetCursorPosition(st->ConOut, (ulong)CursorLeft, (ulong)value);
        }
    } = 0;

    public static bool CursorVisible
    {
        get;
        set
        {
            EfiSystemTable* st = Environment.EfiSysTable;
            field = value;
            if (st == null)
            {
                return;
            }

            st->ConOut->EnableCursor(st->ConOut, value);
        }
    } = false;

    /// <summary>
    /// Sets the foreground color.
    /// </summary>
    public static ConsoleColor ForegroundColor
    {
        get;
        set
        {
            field = value;
            _overallEfiTextColor = (EfiTextColor)((int)BackgroundColor * 0x10) | (EfiTextColor)value;

            EfiSystemTable* st = Environment.EfiSysTable;
            if (st == null)
            {
                return;
            }

            st->ConOut->SetAttribute(st->ConOut, _overallEfiTextColor);
        }
    } = ConsoleColor.White;

    #endregion


    #region Methods

    public static (int Left, int Top) GetCursorPosition()
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        return st == null ? (0, 0) : (st->ConOut->Mode->CursorColumn, st->ConOut->Mode->CursorRow);
    }

    public static ConsoleKeyInfo ReadKey()
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            return default;
        }

        ulong idx = 0;
        st->BootServices->WaitForEvent(1, &st->ConIn->WaitForKey, &idx);

        EfiInputKey input = default;
        st->ConIn->ReadKeyStroke(st->ConIn, &input);

        //TODO: correctly translate the scan code to ConsoleKey
        return new ConsoleKeyInfo(input.UnicodeChar, (ConsoleKey)input.ScanCode, false, false, false);
    }

    public static void WriteLine(string message)
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            _lastStatus = EfiStatus.DeviceError;
            return;
        }

        fixed (char* pMsg = message)
        {
            EfiStatus status = st->ConOut->OutputString(st->ConOut, pMsg);
            if (status != EfiStatus.Success)
            {
                _lastStatus = status;
                return;
            }

            fixed (char* pNewline = NEWLINE)
            {
                status = st->ConOut->OutputString(st->ConOut, pNewline);
                if (status != EfiStatus.Success)
                {
                    _lastStatus = status;
                    return;
                }
            }
        }

        CursorLeft = st->ConOut->Mode->CursorColumn;
        CursorTop = st->ConOut->Mode->CursorRow + 1;
        _lastStatus = EfiStatus.Success;
    }

    public static void Write(string message)
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            _lastStatus = EfiStatus.DeviceError;
            return;
        }

        fixed (char* pMsg = message)
        {
            EfiStatus status = st->ConOut->OutputString(st->ConOut, pMsg);
            if (status != EfiStatus.Success)
            {
                _lastStatus = status;
                return;
            }
        }

        CursorLeft = st->ConOut->Mode->CursorColumn;
        CursorTop = st->ConOut->Mode->CursorRow;
        _lastStatus = EfiStatus.Success;
    }

    public static void WriteRaw(char* message)
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            _lastStatus = EfiStatus.DeviceError;
            return;
        }

        EfiStatus status = st->ConOut->OutputString(st->ConOut, message);
        if (status != EfiStatus.Success)
        {
            _lastStatus = status;
            return;
        }

        CursorLeft = st->ConOut->Mode->CursorColumn;
        CursorTop = st->ConOut->Mode->CursorRow;
        _lastStatus = EfiStatus.Success;
    }

    public static void Clear()
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            return;
        }

        st->ConOut->ClearScreen(st->ConOut);
    }

    #region EFI specific

    public static bool CanOutputString(string testString)
    {
        EfiSystemTable* st = Environment.EfiSysTable;
        if (st == null)
        {
            return false;
        }

        fixed (char* pMsg = testString)
        {
            EfiStatus status = st->ConOut->TestString(st->ConOut, pMsg);
            return status == EfiStatus.Success;
        }
    }

    public static EfiStatus GetLastStatus()
    {
        return _lastStatus;
    }

    #endregion

    #endregion
}

#endif