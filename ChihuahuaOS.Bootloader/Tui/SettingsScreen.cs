using System;
using System.Collections.Generic;
using System.IO;
using ChihuahuaOS.Bootloader.SettingsManager;
using ChihuahuaOS.Bootloader.Tui.ValueSetters;

namespace ChihuahuaOS.Bootloader.Tui;

public static class SettingsScreen
{
    public const int OVERLAY_WIDTH = 60;

    internal static int MainTableHeight;
    internal static int CurrentCursorPosition = 2;
    internal static KernelSettings KernelSettings;

    private static bool _wantsBackNavigation;
    private static OsVersion _osVersion;

    private static bool _isDescriptionOverlayActive;
    private static bool _isValueSettingOverlayActive;
    private static bool _isNotificationPopupActive;
    private static int _overlayHeight;

    private static int _currentScrollValue = 0;

    public static void OnEnterScreen(OsVersion osVersion)
    {
        //reset
        _wantsBackNavigation = false;
        _osVersion = osVersion;
        //we extract the upper padding, the lower table height, then the 3 borders
        MainTableHeight = Console.BufferHeight - TuiRenderer.TOP_TABLE_START - TuiRenderer.LOWER_TABLE_HEIGHT - 3;

        //clear the main area
        TuiRenderer.DrawRect(
            1,
            TuiRenderer.TOP_TABLE_START,
            Console.BufferWidth - 2,
            MainTableHeight,
            ConsoleColor.Black);

        LoadSettings();
        DrawTitle();
        DrawGeneralBottomInstructions();

        SubsectionRenderer.OnEnterScreen();
        SubsectionRenderer.Draw(0, 0, 3);
    }

    public static void DrawMain(ConsoleKeyInfo newKeyStroke)
    {
        bool wasOverlayActive = _isDescriptionOverlayActive || _isValueSettingOverlayActive;
        int descOverlayY = wasOverlayActive ? (Console.BufferHeight - _overlayHeight) / 2 : 0;

        if (_isNotificationPopupActive)
        {
            //TODO: when there are multiple settings, here we need to redraw the UI, not clear it
            TuiRenderer.DrawRect(
                1,
                MainTableHeight + TuiRenderer.TOP_TABLE_START - 1,
                Console.BufferWidth - 2,
                1,
                ConsoleColor.Black);
            _isNotificationPopupActive = false;
        }

        //if the description overlay is active and any key was pressed
        bool shouldRemoveDescription = _isDescriptionOverlayActive && newKeyStroke.Key != ConsoleKey.None;
        //if the value setting overlay is active and the pressed key is Enter or Escape,
        // then remove the overlay and reset state 
        bool shouldRemoveValueSetting =
            _isValueSettingOverlayActive &&
            (newKeyStroke.Key == ConsoleKey.Enter || newKeyStroke.Key == ConsoleKey.Escape);

        if (shouldRemoveValueSetting || shouldRemoveDescription)
        {
            //only set if the pressed key was Enter
            if (shouldRemoveValueSetting && newKeyStroke.Key == ConsoleKey.Enter)
            {
                ListValueSetter.End();
                //TODO: only draw the things that were changed, not the entire screen
                SubsectionRenderer.Draw(0, 0, 3);
            }

            int x = (Console.BufferWidth - OVERLAY_WIDTH) / 2;
            //clear description overlay
            TuiRenderer.DrawRect(
                x,
                descOverlayY,
                OVERLAY_WIDTH,
                _overlayHeight,
                ConsoleColor.Black);

            _isDescriptionOverlayActive = false;
            _isValueSettingOverlayActive = false;
            _overlayHeight = 0;
        }
        else if (_isValueSettingOverlayActive && newKeyStroke.Key != ConsoleKey.Enter)
        {
            ListValueSetter.Draw(newKeyStroke);
            return;
        }

        if (wasOverlayActive)
        {
            if (shouldRemoveValueSetting && newKeyStroke.Key == ConsoleKey.Enter)
            {
                SaveSettings();
            }

            DrawGeneralBottomInstructions();
        }

        //if we just removed an overlay, we don't want to bring react to anything from this screen since
        // it might have been used for something else on the overlay
        if (wasOverlayActive)
        {
            return;
        }

        switch (newKeyStroke.Key)
        {
            case ConsoleKey.Escape:
            {
                if (!wasOverlayActive)
                {
                    _wantsBackNavigation = true;
                }

                return;
            }
            case ConsoleKey.UpArrow
                when CurrentCursorPosition > SubsectionRenderer.GLOBAL_START_POS:
                CurrentCursorPosition--;
                break;
            case ConsoleKey.DownArrow
                when CurrentCursorPosition < SubsectionRenderer.GLOBAL_END_POS:
                CurrentCursorPosition++;
                break;
            case ConsoleKey.I:
                DrawDescriptionOverlay();
                return;
            case ConsoleKey.Enter:
                DrawValueSetterOverlay();
                break;
            default:
                return;
        }

        //TODO: only draw the things that were changed, not the entire screen
        SubsectionRenderer.Draw(0, 0, 3);
    }

    public static bool WantsBackNavigation()
    {
        return _wantsBackNavigation;
    }

    public static bool WantsForcedRedraw()
    {
        return _wantsBackNavigation;
    }

    internal static void DrawIndividualSetting(
        int absoluteStartRowPosition,
        string settingName,
        string settingValue,
        bool isSelected)
    {
        TuiRenderer.DrawRect(
            1, absoluteStartRowPosition, Console.BufferWidth - 2, 1, ConsoleColor.Black);

        Console.CursorTop = absoluteStartRowPosition;
        Console.CursorLeft = 1;
        Console.Write(settingName);

        using string valueRaw = " <" + settingValue + "> ";

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

    internal static void DrawOverlayTitle(string title, int overlayHeight)
    {
        int x = (Console.BufferWidth - OVERLAY_WIDTH) / 2;
        int y = (Console.BufferHeight - overlayHeight) / 2;
        using string horizontalLine = new('\u2500', OVERLAY_WIDTH - 2);

        Console.CursorLeft = x;
        Console.CursorTop = y;
        Console.Write("\u250c");
        Console.Write(horizontalLine);
        Console.Write("\u2510");

        //title
        {
            int titleStart =
                TuiRenderer.AlignText(
                    title,
                    x,
                    x + OVERLAY_WIDTH,
                    TuiRenderer.TextAlignment.Center);
            using string blanks = new(' ', OVERLAY_WIDTH - 2);

            Console.CursorLeft = x;
            Console.CursorTop = y + 1;
            Console.Write("\u2502");
            Console.Write(blanks);
            Console.Write("\u2502");

            Console.CursorLeft = titleStart;
            Console.CursorTop = y + 1;
            Console.Write(title);
        }

        Console.CursorLeft = x;
        Console.CursorTop = y + 2;
        Console.Write("\u251c");
        Console.Write(horizontalLine);
        Console.Write("\u2524");
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

    private static void DrawDescriptionOverlay()
    {
        (string title, string[] description) = SubsectionRenderer.GetTitleAndDescriptionAt(CurrentCursorPosition);

        int height = 4 + description.Length;
        _isDescriptionOverlayActive = true;
        _overlayHeight = height;

        int x = (Console.BufferWidth - OVERLAY_WIDTH) / 2;
        int y = (Console.BufferHeight - height) / 2;
        using string horizontalLine = new('\u2500', OVERLAY_WIDTH - 2);

        Console.BackgroundColor = ConsoleColor.DarkMagenta;
        Console.ForegroundColor = ConsoleColor.White;

        DrawOverlayTitle(title, height);

        //description
        {
            for (int i = 0; i < description.Length; i++)
            {
                using string blanks = new(' ', OVERLAY_WIDTH - 2 - description[i].Length);

                Console.CursorLeft = x;
                Console.CursorTop = y + 3 + i;
                Console.Write("\u2502");
                Console.Write(description[i]);
                Console.Write(blanks);
                Console.Write("\u2502");
            }

            description.Dispose();
        }

        Console.CursorLeft = x;
        Console.CursorTop = y + height - 1;
        Console.Write("\u2514");
        Console.Write(horizontalLine);
        Console.Write("\u2518");

        DrawDescriptionBottomInstructions();
    }

    private static void DrawValueSetterOverlay()
    {
        //TODO: separate selection from textual input
        _isValueSettingOverlayActive = true;
        _overlayHeight = ListValueSetter.Init();

        DrawSelectionValueSetterBottomInstructions();
    }

    private static void LoadSettings()
    {
        using string settingsFilePath = "\\EFI\\BOOT\\ChiOS_" + _osVersion + ".CFG";
        FileStream? fs = File.OpenRead(settingsFilePath);
        if (fs == null)
        {
            using string errCodeString = ((long)File.LastOpenError & 0xffL).ToString();
            using string errorMsg = " ERROR: File open issue (" + errCodeString + ")!";

            Console.CursorLeft = 1;
            Console.CursorTop = MainTableHeight + TuiRenderer.TOP_TABLE_START - 1;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(errorMsg);
            _isNotificationPopupActive = true;
            return;
        }

        List<TomlSetting> settings = TomlManager.ReadFromStream(fs, KernelSettings.NUM_SETTINGS);
        KernelSettings = KernelSettings.FromConfigList(settings);
        settings.Dispose();
    }

    private static void SaveSettings()
    {
        Console.CursorLeft = 1;
        Console.CursorTop = MainTableHeight + TuiRenderer.TOP_TABLE_START - 1;
        Console.ForegroundColor = ConsoleColor.White;

        using string settingsFilePath = "\\EFI\\BOOT\\ChiOS_" + _osVersion + ".CFG";
        FileStream? fs = File.Open(settingsFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        if (fs == null)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            using string errCodeString = ((long)File.LastOpenError & 0xffL).ToString();
            using string errorMsg = " ERROR: File open issue (" + errCodeString + ")!";
            Console.Write(errorMsg);
            _isNotificationPopupActive = true;
            return;
        }

        List<TomlSetting> configList = KernelSettings.ToConfigList();
        bool success = TomlManager.WriteToStream(fs, configList);

        if (success)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write(" Settings saved successfully!");
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(" ERROR: Failed to write settings to file");
        }

        _isNotificationPopupActive = true;
        configList.Dispose();
    }

    private static void DrawGeneralBottomInstructions()
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
        Console.WriteLine("[Up/down arrow] Navigate settings");
        Console.CursorLeft = 2;
        Console.WriteLine("[Enter] Edit setting");
        Console.CursorLeft = 2;
        Console.WriteLine("[i] View setting description");
    }

    private static void DrawDescriptionBottomInstructions()
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
        Console.WriteLine("<Any key> Exit setting description");
    }

    private static void DrawSelectionValueSetterBottomInstructions()
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
        Console.WriteLine("[Esc] Cancel");
        Console.CursorLeft = 2;
        Console.WriteLine("[Up/down arrow] Cycle between options");
        Console.CursorLeft = 2;
        Console.WriteLine("[Enter] Set value");
    }
}