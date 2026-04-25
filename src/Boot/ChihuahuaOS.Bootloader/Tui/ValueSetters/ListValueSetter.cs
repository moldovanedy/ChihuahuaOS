using System;
using ChihuahuaOS.CoreLib.Extra;

namespace ChihuahuaOS.Bootloader.Tui.ValueSetters;

internal static class ListValueSetter
{
    /// <summary>
    /// The number of rows occupied by borders (3), title (1), and scroll arrows (2).
    /// </summary>
    private const int NUM_PADDING_ROWS = 6;

    private const int MAX_ROWS = 10;

    private static int _height;
    private static int _currentIndex;
    private static int _scrolledRows;

    private static StaticRef<string[]> _options;

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The number of rows occupied (height).</returns>
    public static int Init()
    {
        (string title, string[] values) =
            SubsectionRenderer.GetValuesForPropertyAt(SettingsScreen.CurrentCursorPosition);
        _height = NUM_PADDING_ROWS + (values.Length >= MAX_ROWS ? MAX_ROWS : values.Length);
        _currentIndex = 0;
        _scrolledRows = 0;
        _options.SetValue(values);

        string? selectedValue = SubsectionRenderer.GetValueForProperty(SettingsScreen.CurrentCursorPosition);
        if (selectedValue != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (selectedValue == values[i])
                {
                    _currentIndex = i;
                    _scrolledRows = Math.Max(_currentIndex - MAX_ROWS + 2, 0);
                    break;
                }
            }
        }

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
        if (!_options.HasValue())
        {
            return;
        }

        string[] options = _options.GetValue()!;
        SubsectionRenderer.SetValueForProperty(SettingsScreen.CurrentCursorPosition, options[_currentIndex]);

        _height = 0;
        for (int i = 0; i < options.Length; i++)
        {
            options[i].Dispose();
        }

        options.Dispose();
    }

    public static void Draw(ConsoleKeyInfo newKeyStroke)
    {
        if (!_options.HasValue())
        {
            return;
        }

        string[] options = _options.GetValue()!;
        bool needsRedraw = false;
        switch (newKeyStroke.Key)
        {
            case ConsoleKey.DownArrow:
            {
                if (_currentIndex + 1 < options.Length)
                {
                    _currentIndex++;
                    needsRedraw = true;
                }

                if (_currentIndex >= _scrolledRows + MAX_ROWS - 1 && _scrolledRows < options.Length - MAX_ROWS)
                {
                    _scrolledRows++;
                    needsRedraw = true;
                }

                break;
            }
            case ConsoleKey.UpArrow:
            {
                if (_currentIndex - 1 >= 0)
                {
                    _currentIndex--;
                    needsRedraw = true;
                }

                if (_currentIndex - 1 < _scrolledRows && _scrolledRows > 0)
                {
                    _scrolledRows--;
                    needsRedraw = true;
                }

                break;
            }
            case ConsoleKey.None:
                needsRedraw = true;
                break;
        }

        if (!needsRedraw)
        {
            return;
        }

        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.ForegroundColor = ConsoleColor.White;

        int x = (Console.BufferWidth - SettingsScreen.OVERLAY_WIDTH) / 2;
        int y = (Console.BufferHeight - _height) / 2;

        //the start is 28, the end is 29 (avoiding dynamic allocations)
        const string ARROWS_START_BLANKS = "                            ";
        const string ARROWS_END_BLANKS = "                             ";

        //top arrow
        Console.CursorLeft = x + 1;
        Console.CursorTop = y + 3;
        Console.Write(ARROWS_START_BLANKS);
        Console.Write(_scrolledRows > 0 ? "\u25b2" : " ");
        Console.Write(ARROWS_END_BLANKS);

        //bottom arrow
        Console.CursorLeft = x + 1;
        Console.CursorTop = y + _height - 2;
        Console.Write(ARROWS_START_BLANKS);
        Console.Write(_scrolledRows + MAX_ROWS < options.Length ? "\u25bc" : " ");
        Console.Write(ARROWS_END_BLANKS);

        int maxIndex = Math.Min(_scrolledRows + MAX_ROWS, options.Length);
        for (int i = _scrolledRows; i < maxIndex; i++)
        {
            using string blanks = new(' ', SettingsScreen.OVERLAY_WIDTH - 2 - options[i].Length);

            if (_currentIndex == i)
            {
                Console.BackgroundColor = ConsoleColor.DarkCyan;
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.CursorLeft = x + 1;
            Console.CursorTop = y + 4 + (i - _scrolledRows);
            Console.Write(options[i]);
            Console.Write(blanks);

            if (_currentIndex == i)
            {
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}