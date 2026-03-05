using ChihuahuaOS.Bootloader.EfiApi;
using ChihuahuaOS.Bootloader.EfiApi.ConsoleSupport;
using ChihuahuaOS.Bootloader.EfiApi.EfiSysTable;

namespace ChihuahuaOS.Bootloader;

internal static unsafe class ConsoleEfi
{
    #region Properties

    /// <summary>
    /// Sets the background color. Only values that start with "Background" are accepted, otherwise this property will
    /// not change.
    /// </summary>
    public static EfiTextColor BackgroundColor
    {
        get;
        set
        {
            if (((int)value & 0x0f) != 0)
            {
                return;
            }

            field = value;
            _overallTextColor = ForegroundColor | value;

            if (_st == null)
            {
                return;
            }

            _st->ConOut->SetAttribute(_st->ConOut, _overallTextColor);
        }
    } = EfiTextColor.BackgroundBlack;

    public static int BufferHeight => _st == null ? 0 : PrivateGetBufferSize().Rows;

    public static int BufferWidth => _st == null ? 0 : PrivateGetBufferSize().Cols;

    public static int CursorLeft
    {
        get;
        set
        {
            field = value;
            if (_st == null)
            {
                return;
            }

            _st->ConOut->SetCursorPosition(_st->ConOut, (ulong)value, (ulong)CursorTop);
        }
    } = 0;

    public static int CursorTop
    {
        get;
        set
        {
            field = value;
            if (_st == null)
            {
                return;
            }

            _st->ConOut->SetCursorPosition(_st->ConOut, (ulong)CursorLeft, (ulong)value);
        }
    } = 0;

    public static bool CursorVisible
    {
        get;
        set
        {
            field = value;
            if (_st == null)
            {
                return;
            }

            _st->ConOut->EnableCursor(_st->ConOut, value);
        }
    } = false;

    /// <summary>
    /// Sets the foreground color. Only values that don't start with "Background" are accepted, otherwise
    /// this property will not change.
    /// </summary>
    public static EfiTextColor ForegroundColor
    {
        get;
        set
        {
            if (((int)value & 0xf0) != 0)
            {
                return;
            }

            field = value;
            _overallTextColor = BackgroundColor | value;

            if (_st == null)
            {
                return;
            }

            _st->ConOut->SetAttribute(_st->ConOut, _overallTextColor);
        }
    } = EfiTextColor.White;

    private static EfiTextColor _overallTextColor = EfiTextColor.White;

    #endregion


    private static EfiSystemTable* _st = null;
    private const string NEWLINE = "\r\n";


    #region Methods

    public static (int Left, int Top) GetCursorPosition()
    {
        return _st == null ? (0, 0) : (_st->ConOut->Mode->CursorColumn, _st->ConOut->Mode->CursorRow);
    }

    public static EfiInputKey ReadKey()
    {
        if (_st == null)
        {
            return default;
        }

        ulong idx = 0;
        _st->BootServices->WaitForEvent(1, &_st->ConIn->WaitForKey, &idx);

        EfiInputKey input = default;
        _st->ConIn->ReadKeyStroke(_st->ConIn, &input);
        return input;
    }

    public static bool CanOutputString(string testString)
    {
        if (_st == null)
        {
            return false;
        }

        fixed (char* pMsg = testString)
        {
            EfiStatus status = _st->ConOut->TestString(_st->ConOut, pMsg);
            return status == EfiStatus.Success;
        }
    }

    public static EfiStatus WriteLine(string message)
    {
        if (_st == null)
        {
            return EfiStatus.DeviceError;
        }

        fixed (char* pMsg = message)
        {
            EfiStatus status = _st->ConOut->OutputString(_st->ConOut, pMsg);
            if (status != EfiStatus.Success)
            {
                return status;
            }

            fixed (char* pNewline = NEWLINE)
            {
                status = _st->ConOut->OutputString(_st->ConOut, pNewline);
                if (status != EfiStatus.Success)
                {
                    return status;
                }
            }
        }

        CursorLeft = _st->ConOut->Mode->CursorColumn;
        CursorTop = _st->ConOut->Mode->CursorRow;
        return EfiStatus.Success;
    }

    public static EfiStatus Write(string message)
    {
        if (_st == null)
        {
            return EfiStatus.DeviceError;
        }

        fixed (char* pMsg = message)
        {
            EfiStatus status = _st->ConOut->OutputString(_st->ConOut, pMsg);
            if (status != EfiStatus.Success)
            {
                return status;
            }
        }

        CursorLeft = _st->ConOut->Mode->CursorColumn;
        CursorTop = _st->ConOut->Mode->CursorRow;
        return EfiStatus.Success;
    }

    public static EfiStatus WriteRaw(char* message)
    {
        if (_st == null)
        {
            return EfiStatus.DeviceError;
        }

        EfiStatus status = _st->ConOut->OutputString(_st->ConOut, message);
        if (status != EfiStatus.Success)
        {
            return status;
        }

        CursorLeft = _st->ConOut->Mode->CursorColumn;
        CursorTop = _st->ConOut->Mode->CursorRow;
        return EfiStatus.Success;
    }

    public static void Clear()
    {
        if (_st == null)
        {
            return;
        }

        _st->ConOut->ClearScreen(_st->ConOut);
    }

    #endregion

    internal static void SetSystemTableReference(EfiSystemTable* systemTable)
    {
        _st = systemTable;
    }

    private static (int Cols, int Rows) PrivateGetBufferSize()
    {
        ulong row = 0;
        ulong col = 0;
        EfiStatus status = _st->ConOut->QueryMode(_st->ConOut, (ulong)_st->ConOut->Mode->Mode, &col, &row);
        return
            status != EfiStatus.Success
                ? (80, 25)
                : (col != 0 ? (int)col : 80, row != 0 ? (int)row : 25);
    }
}