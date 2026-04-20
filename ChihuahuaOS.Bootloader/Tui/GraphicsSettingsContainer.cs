using System;

namespace ChihuahuaOS.Bootloader.Tui;

internal static class GraphicsSettingsContainer
{
    public const int GLOBAL_ROW_START = 0;
    public const int GLOBAL_ROW_END = 2;

    public const int GLOBAL_START_POS = 2;
    public const int GLOBAL_END_POS = 2;

    private const int PREFERRED_SCREEN_SIZE_Y_POS = GLOBAL_START_POS;
    private const string PREFERRED_SCREEN_SIZE_NAME = "Preferred screen size";

    public static void OnEnterScreen()
    {
        //this is used to list the GOP modes info

        // using Gop.GopModeInfoEnumerator enumerator = Gop.GetModeInfoEnumerator();
        // while (enumerator.MoveNext())
        // {
        //     Console.CursorLeft = 1;
        //     Console.Write(enumerator.Current.HorizontalResolution.ToString());
        // }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalCursorRowPosition"></param>
    /// <returns>
    /// The title and the rows of the description. You should dispose of the description array, but not its values.
    /// </returns>
    public static (string Title, string[] Description) GetTitleAndDescriptionAt(int globalCursorRowPosition)
    {
        //width is 58 maximum!
        switch (globalCursorRowPosition)
        {
            case PREFERRED_SCREEN_SIZE_Y_POS:
                return (
                    PREFERRED_SCREEN_SIZE_NAME,
                    [
                        "Sets the preferred screen size. The OS will try to respect",
                        "this value or pick the closest one to this."
                    ]);
            default:
                return ("Unknown", ["Internal error"]);
        }
    }

    public static (string Title, string[] Values) GetValuesForPropertyAt(int globalCursorRowPosition)
    {
        //width is 58 maximum!
        switch (globalCursorRowPosition)
        {
            case PREFERRED_SCREEN_SIZE_Y_POS:
                return (
                    PREFERRED_SCREEN_SIZE_NAME,
                    [
                        //TODO: replace with dynamically retrieved values
                        "960x540",
                        "1280x720",
                        "1920x1080"
                    ]);
            default:
                return ("Unknown", ["Internal error"]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="globalScrollRowPosition">The number of rows scrolled (global position).</param>
    /// <param name="globalRedrawRowPosition">The global row where to start redrawing.</param>
    /// <param name="rowsToRedraw">The number of rows to redraw.</param>
    public static void Draw(
        int globalScrollRowPosition,
        int globalRedrawRowPosition,
        int rowsToRedraw)
    {
        if (rowsToRedraw == 0 || globalScrollRowPosition > GLOBAL_ROW_END)
        {
            return;
        }

        int absoluteRowStart = GLOBAL_ROW_START - globalScrollRowPosition + TuiRenderer.TOP_TABLE_START;
        if (absoluteRowStart - TuiRenderer.TOP_TABLE_START > SettingsScreen.MainTableHeight)
        {
            return;
        }

        int globalEndDrawPosition = globalRedrawRowPosition + rowsToRedraw;
        if (globalEndDrawPosition >= GLOBAL_ROW_START)
        {
            DrawSubTitle(absoluteRowStart);
        }

        int relYPos = globalEndDrawPosition - GLOBAL_ROW_START;
        switch (relYPos)
        {
            case >= PREFERRED_SCREEN_SIZE_Y_POS:
                SettingsScreen.DrawIndividualSetting(
                    absoluteRowStart + PREFERRED_SCREEN_SIZE_Y_POS,
                    PREFERRED_SCREEN_SIZE_NAME,
                    "1920x1080",
                    true);
                break;
        }
    }


    private static void DrawSubTitle(int absoluteRowPosition)
    {
        TuiRenderer.DrawRect(
            1, absoluteRowPosition, Console.BufferWidth - 2, 1, ConsoleColor.DarkBlue);

        Console.CursorLeft = 1;
        Console.CursorTop = absoluteRowPosition;
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("GRAPHICS");
    }
}