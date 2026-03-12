using System;

namespace ChihuahuaOS.Bootloader.Tui;

public static class SettingsScreen
{
    private static bool _wantsBackNavigation;
    private static OsVersion _osVersion;

    public static void OnEnterScreen(OsVersion osVersion)
    {
        //reset
        _wantsBackNavigation = false;
        _osVersion = osVersion;

        //clear the main area
        TuiRenderer.DrawRect(
            1,
            TuiRenderer.TOP_TABLE_START,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            Console.BufferHeight - TuiRenderer.TOP_TABLE_START - TuiRenderer.LOWER_TABLE_HEIGHT - 3,
            ConsoleColor.Black);

        Console.CursorLeft = 1;
        Console.CursorTop = TuiRenderer.TOP_TABLE_START;
        using string osVerString = osVersion.ToString();
        Console.Write(osVerString);

        DrawTitle();
        DrawBottomInstructions();
    }

    public static void DrawMain(ConsoleKeyInfo newKeyStroke)
    {
        if (newKeyStroke.Key == ConsoleKey.Escape)
        {
            _wantsBackNavigation = true;
            return;
        }

        //TODO: only draw the things that were changed, not the entire screen
        DrawGraphicsSettings();
    }

    public static bool WantsBackNavigation()
    {
        return _wantsBackNavigation;
    }

    public static bool WantsForcedRedraw()
    {
        return _wantsBackNavigation;
    }


    private static void DrawGraphicsSettings()
    {
        TuiRenderer.DrawRect(
            1, TuiRenderer.TOP_TABLE_START, Console.BufferWidth - 2, 1, ConsoleColor.DarkBlue);

        Console.CursorLeft = 1;
        Console.CursorTop = TuiRenderer.TOP_TABLE_START;
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("GRAPHICS");


        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorTop = TuiRenderer.TOP_TABLE_START + 2;

        DrawIndividualOption("Preferred screen size:", "1920x1080", true);
    }

    private static void DrawIndividualOption(string name, string value, bool isSelected = false)
    {
        TuiRenderer.DrawRect(
            1, Console.CursorTop, Console.BufferWidth - 2, 1, ConsoleColor.Black);

        Console.CursorLeft = 1;
        Console.Write(name);

        using string valueRaw = " <" + value + "> ";

        int col = TuiRenderer.AlignText(
            valueRaw, 1, Console.BufferWidth - 2, TuiRenderer.TextAlignment.Right);
        Console.CursorLeft = col;
        if (isSelected)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
        }

        Console.Write(valueRaw);
        Console.BackgroundColor = ConsoleColor.Black;
    }

    private static void DrawBottomInstructions()
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;

        int topLimit = Console.BufferHeight - 2 - TuiRenderer.LOWER_TABLE_HEIGHT;
        Console.CursorTop = topLimit;

        //clear
        TuiRenderer.DrawRect(
            1,
            topLimit,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            TuiRenderer.LOWER_TABLE_HEIGHT,
            ConsoleColor.Black);

        Console.CursorLeft = 2;
        Console.WriteLine("[Esc] Exit settings");
        Console.CursorLeft = 2;
        Console.WriteLine("\u2191 \u2193  [Up/down arrow] Navigate settings");
        Console.CursorLeft = 2;
        Console.WriteLine("\u23ce  [Enter] Edit setting");
    }

    private static void DrawTitle()
    {
        TuiRenderer.DrawRect(0, 0, Console.BufferWidth, 2, ConsoleColor.DarkBlue);

        using string osVerString = _osVersion.ToString();
        using string title = TuiRenderer.CHIHUAHUA_OS + " v. " + osVerString + " - Settings";
        int col = TuiRenderer.AlignText(
            title, 0, Console.BufferWidth, TuiRenderer.TextAlignment.Center);

        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorLeft = col;
        Console.CursorTop = 1;
        Console.Write(title);
    }
}