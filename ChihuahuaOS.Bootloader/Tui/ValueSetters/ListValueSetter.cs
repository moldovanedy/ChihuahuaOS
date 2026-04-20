using System;

namespace ChihuahuaOS.Bootloader.Tui.ValueSetters;

internal static class ListValueSetter
{
    /// <summary>
    /// The number of rows occupied by borders (3), title (1), and scroll arrows (2).
    /// </summary>
    private const int NUM_PADDING_ROWS = 6;

    private static int _height = 15;
    private static string[]? _values = [];

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The number of rows occupied (height).</returns>
    public static int Init()
    {
        const int MAX_ROWS = 10;

        (string title, string[] values) =
            GraphicsSettingsContainer.GetValuesForPropertyAt(SettingsScreen.CurrentCursorPosition);
        _height = NUM_PADDING_ROWS + (values.Length >= MAX_ROWS ? MAX_ROWS : values.Length);
        _values = values;

        int x = (Console.BufferWidth - SettingsScreen.OVERLAY_WIDTH) / 2;
        int y = (Console.BufferHeight - _height) / 2;
        using string horizontalLine = new('\u2500', SettingsScreen.OVERLAY_WIDTH - 2);

        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.ForegroundColor = ConsoleColor.White;

        SettingsScreen.DrawOverlayTitle(title, _height);

        //draw lateral borders
        for (int i = 0; i < _height - NUM_PADDING_ROWS + 2; i++)
        {
            Console.CursorTop = y + 3 + i;

            Console.CursorLeft = x;
            Console.Write("\u2502");
            Console.CursorLeft = x + SettingsScreen.OVERLAY_WIDTH - 1;
            Console.Write("\u2502");
        }

        Draw(new ConsoleKeyInfo());

        Console.CursorLeft = x;
        Console.CursorTop = y + _height - 1;
        Console.Write("\u2514");
        Console.Write(horizontalLine);
        Console.Write("\u2518");
        return _height;
    }

    public static void End()
    {
        _height = 0;
        // _values?.Dispose();
    }

    public static void Draw(ConsoleKeyInfo newKeyStroke)
    {
        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.ForegroundColor = ConsoleColor.White;

        int x = (Console.BufferWidth - SettingsScreen.OVERLAY_WIDTH) / 2;
        int y = (Console.BufferHeight - _height) / 2;

        //the start is 28, the end is 29 (avoiding dynamic allocations)
        const string ARROWS_START_BLANKS = "                            ";
        const string ARROWS_END_BLANKS = "                             ";

        //top and bottom arrows 
        Console.CursorLeft = x + 1;
        Console.CursorTop = y + 3;
        Console.Write(ARROWS_START_BLANKS);
        Console.Write("\u25b2");
        Console.Write(ARROWS_END_BLANKS);

        Console.CursorLeft = x + 1;
        Console.CursorTop = y + _height - 2;
        Console.Write(ARROWS_START_BLANKS);
        Console.Write("\u25bc");
        Console.Write(ARROWS_END_BLANKS);

        if (_values == null)
        {
            return;
        }

        for (int i = 0; i < _values.Length; i++)
        {
            using string blanks = new(' ', SettingsScreen.OVERLAY_WIDTH - 2 - _values[i].Length);

            Console.CursorLeft = x + 1;
            Console.CursorTop = y + 4 + i;
            Console.Write(_values[i]);
            Console.Write(blanks);
        }
    }
}