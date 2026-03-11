using System;

namespace ChihuahuaOS.Bootloader.Tui;

internal static class MainScreen
{
    private static int _currentOption;

    public static void OnEnterScreen()
    {
        //TODO: actually check and list all OS versions, as well as check whether booting to EFI
        // is supported (OsIndications)

        //clear the main area
        TuiRenderer.DrawRect(
            1,
            TuiRenderer.TOP_TABLE_START,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            Console.BufferHeight - TuiRenderer.TOP_TABLE_START - TuiRenderer.LOWER_TABLE_HEIGHT - 3,
            ConsoleColor.Black);

        RedrawOption(0, true);
        RedrawOption(1, false);
        RedrawOption(2, false);
        RedrawDescription(0);

        DrawBottomInstructions();
    }

    public static void DrawMain(ConsoleKeyInfo newKeyStroke)
    {
        int previousOption = _currentOption;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (newKeyStroke.Key == ConsoleKey.UpArrow && _currentOption > 0)
        {
            _currentOption--;
        }
        else if (newKeyStroke.Key == ConsoleKey.DownArrow && _currentOption < 2)
        {
            _currentOption++;
        }

        if (_currentOption == previousOption)
        {
            return;
        }

        RedrawOption(previousOption, false);
        RedrawOption(_currentOption, true);
        RedrawDescription(_currentOption);
    }

    public static bool WantsForcedRedraw()
    {
        return false;
    }


    private static void RedrawOption(int index, bool isHighlighted)
    {
        Console.CursorLeft = 1;
        Console.CursorTop = TuiRenderer.TOP_TABLE_START + index;
        int charsDrawn = 0;

        if (isHighlighted)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(" ->");
            charsDrawn += 3;
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        switch (index)
        {
            case 0:
            {
                const string OPTION_1 = " Start ChihuahuaOS v.0.1.0";
                Console.Write(OPTION_1);
                charsDrawn += OPTION_1.Length;
                break;
            }
            case 1:
            {
                const string OPTION_2 = " Enter OS boot settings (for v.0.1.0)";
                Console.Write(OPTION_2);
                charsDrawn += OPTION_2.Length;
                break;
            }
            case 2:
            {
                const string OPTION_3 = " Enter UEFI firmware settings";
                Console.Write(OPTION_3);
                charsDrawn += OPTION_3.Length;
                break;
            }
        }

        int whiteSpaceLength = Console.BufferWidth - 2 - charsDrawn;
        if (whiteSpaceLength < 0)
        {
            return;
        }

        using string ws = new(' ', whiteSpaceLength);
        Console.Write(ws);
    }

    private static void RedrawDescription(int index)
    {
        //we extract the lower table height, then the 2 borders, then 3 rows for this description
        int startRow = Console.BufferHeight - 1 - TuiRenderer.LOWER_TABLE_HEIGHT - 2 - 3;
        CleanupDescription(startRow);

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorLeft = 1;
        Console.CursorTop = startRow;

        Console.Write(" INFO: ");
        switch (index)
        {
            case 0:
            {
                const string OPTION_DESC_1 = "Starts Chihuahua OS (version 0.1.0).";
                Console.Write(OPTION_DESC_1);
                break;
            }
            case 1:
            {
                const string OPTION_DESC_2 =
                    "Configure the boot settings for ChihuahuaOS (version 0.1.0), settings that might impact the " +
                    "mode the OS boots.";
                Console.Write(OPTION_DESC_2);
                break;
            }
            case 2:
            {
                const string OPTION_DESC_3 =
                    "Reboot the system and enter into the device's firmware settings (not supported on all devices).";
                Console.Write(OPTION_DESC_3);
                break;
            }
        }
    }

    private static void CleanupDescription(int startRow)
    {
        //clear
        TuiRenderer.DrawRect(
            1,
            startRow,
            Console.BufferWidth - 2,
            3,
            ConsoleColor.Black);

        //redraw borders (if the text overflows, artifacts will remain otherwise)
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.CursorLeft = 0;
        Console.CursorTop = startRow - 1;
        Console.Write("\u251c");

        Console.CursorLeft = Console.BufferWidth - 1;
        Console.CursorTop = startRow - 1;
        Console.Write("\u2524");

        using string horLine = new('\u2500', Console.BufferWidth - 2);
        Console.CursorLeft = 1;
        Console.CursorTop = startRow - 1;
        Console.Write(horLine);

        for (int i = startRow; i < startRow + 3; i++)
        {
            Console.CursorLeft = 0;
            Console.CursorTop = i;
            Console.Write("\u2502");

            Console.CursorLeft = Console.BufferWidth - 1;
            Console.CursorTop = i;
            Console.Write("\u2502");
        }
    }

    private static void DrawBottomInstructions()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;

        int topLimit = Console.BufferHeight - 2 - TuiRenderer.LOWER_TABLE_HEIGHT;
        Console.CursorTop = topLimit;

        Console.CursorLeft = 2;
        Console.WriteLine("\u2191 \u2193  [Up/down arrow] Navigate options");
        Console.CursorLeft = 2;
        Console.WriteLine("\u23ce  [Enter] Select option");
    }
}