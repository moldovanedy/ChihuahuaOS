using ChihuahuaOS.Bootloader.EfiApi.ConsoleSupport;

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
        DrawRect(0, 0, ConsoleEfi.BufferWidth, 2, EfiTextColor.BackgroundGreen);

        const string CHIHUAHUA_OS = "ChihuahuaOS";
        int col = AlignText(CHIHUAHUA_OS, 0, ConsoleEfi.BufferWidth, TextAlignment.Center);

        ConsoleEfi.BackgroundColor = EfiTextColor.BackgroundGreen;
        ConsoleEfi.ForegroundColor = EfiTextColor.Black;
        ConsoleEfi.CursorLeft = col;
        ConsoleEfi.CursorTop = 1;
        ConsoleEfi.Write(CHIHUAHUA_OS);

        ConsoleEfi.BackgroundColor = EfiTextColor.BackgroundBlack;
        ConsoleEfi.ForegroundColor = EfiTextColor.LightGray;
        DrawTableMainBorders();
        DrawTableCorners();

        DrawRect(
            1,
            ConsoleEfi.BufferHeight - LOWER_TABLE_HEIGHT - 2,
            ConsoleEfi.BufferWidth - 2,
            LOWER_TABLE_HEIGHT,
            EfiTextColor.BackgroundBlack);
    }

    public static void RedrawBottomInstructions()
    {
        int topLimit = ConsoleEfi.BufferHeight - 2 - LOWER_TABLE_HEIGHT;

        ConsoleEfi.CursorLeft = 2;
        ConsoleEfi.CursorTop = topLimit;
        if (ConsoleEfi.CanOutputString("\u2191") && ConsoleEfi.CanOutputString("\u2193"))
        {
            ConsoleEfi.WriteLine("\u2191 \u2193  Navigate options");
        }
        else
        {
            ConsoleEfi.WriteLine("[Up/down arrow] Navigate options");
        }

        ConsoleEfi.CursorLeft = 2;
        ConsoleEfi.CursorTop = topLimit + 1;
        ConsoleEfi.WriteLine(ConsoleEfi.CanOutputString("\u23ce") ? "\u23ce  Select option" : "[Enter] Select option");
    }

    public static void RedrawMainContent()
    {
        DrawRect(
            1,
            TOP_TABLE_START,
            ConsoleEfi.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            ConsoleEfi.BufferHeight - TOP_TABLE_START - LOWER_TABLE_HEIGHT - 3,
            EfiTextColor.BackgroundBlack);
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
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color">Only background colors are accepted!</param>
    public static unsafe void DrawRect(int x, int y, int width, int height, EfiTextColor color)
    {
        //stack allocation is only available to this method; do not use outside 
        char* horizontalLine = stackalloc char[300];
        for (int i = 0; i < width; i++)
        {
            horizontalLine[i] = ' ';
        }

        horizontalLine[width] = '\0';
        EfiTextColor previousColor = ConsoleEfi.BackgroundColor;
        int previousCol = ConsoleEfi.CursorLeft;
        int previousRow = ConsoleEfi.CursorTop;

        ConsoleEfi.BackgroundColor = color;
        for (int i = 0; i < height; i++)
        {
            ConsoleEfi.CursorLeft = x;
            ConsoleEfi.CursorTop = y + i;
            ConsoleEfi.WriteRaw(horizontalLine);
        }

        ConsoleEfi.BackgroundColor = previousColor;
        ConsoleEfi.CursorLeft = previousCol;
        ConsoleEfi.CursorTop = previousRow;
    }


    private static unsafe void DrawTableMainBorders()
    {
        //stack allocation is only available to this method; do not use outside 
        char* horizontalLine = stackalloc char[300];
        int width = ConsoleEfi.BufferWidth - 2;
        for (int i = 0; i < width; i++)
        {
            horizontalLine[i] = '\u2500';
        }

        horizontalLine[width] = '\0';

        for (int table = 0; table < 2; table++)
        {
            //TOP_TABLE_START is the padding from above, 3 is the padding from below
            int height = table == 0 ? ConsoleEfi.BufferHeight - TOP_TABLE_START - 3 : 4;

            int top = table == 0 ? TOP_TABLE_START - 1 : ConsoleEfi.BufferHeight - LOWER_TABLE_HEIGHT - 3;
            int bottom = table == 0 ? height : ConsoleEfi.BufferHeight - 2;

            ConsoleEfi.CursorLeft = 1;
            ConsoleEfi.CursorTop = top;
            ConsoleEfi.WriteRaw(horizontalLine);

            ConsoleEfi.CursorLeft = 1;
            ConsoleEfi.CursorTop = bottom;
            ConsoleEfi.WriteRaw(horizontalLine);

            for (int i = top + 1; i < bottom; i++)
            {
                ConsoleEfi.CursorLeft = 0;
                ConsoleEfi.CursorTop = i;
                ConsoleEfi.Write("\u2502");
            }

            for (int i = top + 1; i < bottom; i++)
            {
                ConsoleEfi.CursorLeft = ConsoleEfi.BufferWidth - 1;
                ConsoleEfi.CursorTop = i;
                ConsoleEfi.Write("\u2502");
            }
        }
    }

    private static void DrawTableCorners()
    {
        //order is: top-left, top-right, bottom-right, bottom-left, left connector, right connector 
        ConsoleEfi.CursorLeft = 0;
        ConsoleEfi.CursorTop = TOP_TABLE_START - 1;
        ConsoleEfi.Write("\u250c");

        ConsoleEfi.CursorLeft = ConsoleEfi.BufferWidth - 1;
        ConsoleEfi.CursorTop = TOP_TABLE_START - 1;
        ConsoleEfi.Write("\u2510");

        ConsoleEfi.CursorLeft = ConsoleEfi.BufferWidth - 1;
        ConsoleEfi.CursorTop = ConsoleEfi.BufferHeight - 2;
        ConsoleEfi.Write("\u2518");

        ConsoleEfi.CursorLeft = 0;
        ConsoleEfi.CursorTop = ConsoleEfi.BufferHeight - 2;
        ConsoleEfi.Write("\u2514");


        ConsoleEfi.CursorLeft = 0;
        ConsoleEfi.CursorTop = ConsoleEfi.BufferHeight - LOWER_TABLE_HEIGHT - 3;
        ConsoleEfi.Write("\u251c");

        ConsoleEfi.CursorLeft = ConsoleEfi.BufferWidth - 1;
        ConsoleEfi.CursorTop = ConsoleEfi.BufferHeight - LOWER_TABLE_HEIGHT - 3;
        ConsoleEfi.Write("\u2524");
    }
}