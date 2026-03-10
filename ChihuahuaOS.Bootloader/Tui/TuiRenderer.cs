using System;

namespace ChihuahuaOS.Bootloader.Tui;

internal static class TuiRenderer
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    private const int LOWER_TABLE_HEIGHT = 4;
    private const int TOP_TABLE_START = 3;

    public static void DrawPersistentElements()
    {
        DrawRect(0, 0, Console.BufferWidth, 2, ConsoleColor.DarkGreen);

        const string CHIHUAHUA_OS = "ChihuahuaOS";
        int col = AlignText(CHIHUAHUA_OS, 0, Console.BufferWidth, TextAlignment.Center);

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.CursorLeft = col;
        Console.CursorTop = 1;
        Console.Write(CHIHUAHUA_OS);

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Gray;
        DrawTableMainBorders();
        DrawTableCorners();

        DrawRect(
            1,
            Console.BufferHeight - LOWER_TABLE_HEIGHT - 2,
            Console.BufferWidth - 2,
            LOWER_TABLE_HEIGHT,
            ConsoleColor.Black);
    }

    public static void RedrawBottomInstructions()
    {
        int topLimit = Console.BufferHeight - 2 - LOWER_TABLE_HEIGHT;
        Console.CursorTop = topLimit;

        Console.CursorLeft = 2;
        Console.WriteLine("\u2191 \u2193  [Up/down arrow] Navigate options");
        Console.CursorLeft = 2;
        Console.WriteLine("\u23ce  [Enter] Select option");
    }

    public static void RedrawMainContent()
    {
        DrawRect(
            1,
            TOP_TABLE_START,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            Console.BufferHeight - TOP_TABLE_START - LOWER_TABLE_HEIGHT - 3,
            ConsoleColor.Black);
    }

    public static int AlignText(string text, int startCol, int endCol, TextAlignment alignment)
    {
        switch (alignment)
        {
            default:
            case TextAlignment.Left:
                return startCol;
            case TextAlignment.Center:
                int pos = (startCol + endCol) / 2 - text.Length / 2;
                return pos > 0 ? pos : 0;
            case TextAlignment.Right:
                return endCol - text.Length > 0 ? endCol - text.Length : 0;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color">Only background colors are accepted!</param>
    public static unsafe void DrawRect(int x, int y, int width, int height, ConsoleColor color)
    {
        //stack allocation is only available to this method; do not use outside (better as new string because it avoids
        // a memory allocation)
        char* horizontalLine = stackalloc char[300];
        for (int i = 0; i < width; i++)
        {
            horizontalLine[i] = ' ';
        }

        horizontalLine[width] = '\0';

        ConsoleColor previousColor = Console.BackgroundColor;
        int previousCol = Console.CursorLeft;
        int previousRow = Console.CursorTop;

        Console.BackgroundColor = color;
        for (int i = 0; i < height; i++)
        {
            Console.CursorLeft = x;
            Console.CursorTop = y + i;
            Console.WriteRaw(horizontalLine);
        }

        Console.BackgroundColor = previousColor;
        Console.CursorLeft = previousCol;
        Console.CursorTop = previousRow;
    }


    private static unsafe void DrawTableMainBorders()
    {
        int width = Console.BufferWidth - 2;
        //stack allocation is only available to this method; do not use outside (better as new string because it avoids
        // a memory allocation)
        char* horizontalLine = stackalloc char[300];
        for (int i = 0; i < width; i++)
        {
            horizontalLine[i] = '\u2500';
        }

        horizontalLine[width] = '\0';

        for (int table = 0; table < 2; table++)
        {
            //TOP_TABLE_START is the padding from above, 3 is the padding from below
            int height = table == 0 ? Console.BufferHeight - TOP_TABLE_START - 3 : 4;

            int top = table == 0 ? TOP_TABLE_START - 1 : Console.BufferHeight - LOWER_TABLE_HEIGHT - 3;
            int bottom = table == 0 ? height : Console.BufferHeight - 2;

            Console.CursorLeft = 1;
            Console.CursorTop = top;
            Console.WriteRaw(horizontalLine);

            Console.CursorLeft = 1;
            Console.CursorTop = bottom;
            Console.WriteRaw(horizontalLine);

            for (int i = top + 1; i < bottom; i++)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = i;
                Console.Write("\u2502");
            }

            for (int i = top + 1; i < bottom; i++)
            {
                Console.CursorLeft = Console.BufferWidth - 1;
                Console.CursorTop = i;
                Console.Write("\u2502");
            }
        }
    }

    private static void DrawTableCorners()
    {
        //order is: top-left, top-right, bottom-right, bottom-left, left connector, right connector 
        Console.CursorLeft = 0;
        Console.CursorTop = TOP_TABLE_START - 1;
        Console.Write("\u250c");

        Console.CursorLeft = Console.BufferWidth - 1;
        Console.CursorTop = TOP_TABLE_START - 1;
        Console.Write("\u2510");

        Console.CursorLeft = Console.BufferWidth - 1;
        Console.CursorTop = Console.BufferHeight - 2;
        Console.Write("\u2518");

        Console.CursorLeft = 0;
        Console.CursorTop = Console.BufferHeight - 2;
        Console.Write("\u2514");


        Console.CursorLeft = 0;
        Console.CursorTop = Console.BufferHeight - LOWER_TABLE_HEIGHT - 3;
        Console.Write("\u251c");

        Console.CursorLeft = Console.BufferWidth - 1;
        Console.CursorTop = Console.BufferHeight - LOWER_TABLE_HEIGHT - 3;
        Console.Write("\u2524");
    }
}