using System;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.EfiApi.ConsoleSupport;

namespace ChihuahuaOS.Bootloader.Tui.Subsections;

internal class GraphicsSubsection : ISubsection
{
    public const int GLOBAL_ROW_START = 0;
    public const int GLOBAL_ROW_END = 2;

    public const int GLOBAL_START_POS = 2;
    public const int GLOBAL_END_POS = 2;

    private const int PREFERRED_SCREEN_SIZE_Y_POS = GLOBAL_START_POS;
    private const string PREFERRED_SCREEN_SIZE_NAME = "Preferred screen size";

    private static readonly (string Title, string[] Values) ErrorValue = (
        "Unknown",
        ["Internal error"]);

    public void OnEnterScreen()
    {
    }

    public (string Title, string[] Description) GetTitleAndDescriptionAt(int globalCursorRowPosition)
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

    public (string Title, string[] Values) GetValuesForPropertyAt(int globalCursorRowPosition)
    {
        //width is 58 maximum!
        switch (globalCursorRowPosition)
        {
            case PREFERRED_SCREEN_SIZE_Y_POS:
            {
                Gop.GopModeInfoEnumerator? gopModeEnumeratorOption = Gop.GetModeInfoEnumerator();
                if (gopModeEnumeratorOption == null)
                {
                    return ErrorValue;
                }

                Gop.GopModeInfoEnumerator gopModeEnumerator = gopModeEnumeratorOption.Value;

                int arrayLen = Gop.GetModeCount();
                string[] values = new string[arrayLen];
                int index = 0;

                while (gopModeEnumerator.MoveNext() && index < arrayLen)
                {
                    EfiGopModeInformation mode = gopModeEnumerator.Current;
                    if (mode.PixelFormat == EfiGraphicsPixelFormat.PixelBltOnly)
                    {
                        continue;
                    }

                    values[index] = mode.HorizontalResolution + "x" + mode.VerticalResolution;
                    index++;
                }

                return (PREFERRED_SCREEN_SIZE_NAME, values);
            }
            default:
                return ErrorValue;
        }
    }

    public string? GetValueForProperty(int globalCursorRowPosition)
    {
        switch (globalCursorRowPosition)
        {
            case PREFERRED_SCREEN_SIZE_Y_POS:
            {
                return SettingsScreen.KernelSettings.ScreenWidth + "x" + SettingsScreen.KernelSettings.ScreenHeight;
            }
            default:
                return null;
        }
    }

    public void SetValueForProperty(int globalCursorRowPosition, string newValue)
    {
        switch (globalCursorRowPosition)
        {
            case PREFERRED_SCREEN_SIZE_Y_POS:
            {
                int separatorIdx = newValue.IndexOf('x');
                if (separatorIdx == -1)
                {
                    return;
                }

                using string widthStr = newValue.Substring(0, separatorIdx);
                if (int.TryParse(widthStr, out int width))
                {
                    SettingsScreen.KernelSettings.ScreenWidth = width;
                }

                using string heightStr = newValue.Substring(separatorIdx + 1, newValue.Length - separatorIdx - 1);
                if (int.TryParse(heightStr, out int height))
                {
                    SettingsScreen.KernelSettings.ScreenHeight = height;
                }

                break;
            }
        }
    }

    public void Draw(int globalScrollRowPosition, int globalRedrawRowPosition, int rowsToRedraw)
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
            {
                using string settingStr =
                    SettingsScreen.KernelSettings.ScreenWidth + "x" + SettingsScreen.KernelSettings.ScreenHeight;
                SettingsScreen.DrawIndividualSetting(
                    absoluteRowStart + PREFERRED_SCREEN_SIZE_Y_POS,
                    PREFERRED_SCREEN_SIZE_NAME,
                    settingStr,
                    true);
                break;
            }
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