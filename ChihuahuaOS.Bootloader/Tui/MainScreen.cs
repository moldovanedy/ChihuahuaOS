using System;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.RuntimeServices;

namespace ChihuahuaOS.Bootloader.Tui;

internal static class MainScreen
{
    private static int _currentOption;
    private static bool _wantsBootAbort;
    private static bool _supportsBootToFw;

    private static OsVersion? _osVersion;

    private const ulong EFI_OS_INDICATIONS_BOOT_TO_FW_UI = 0x01;

    public static void OnEnterScreen()
    {
        //TODO: actually check and list all OS versions
        CheckSupportForBootToFw();
        DrawTitle();

        //reset
        _osVersion = null;
        _wantsBootAbort = false;

        //clear the main area
        TuiRenderer.DrawRect(
            1,
            TuiRenderer.TOP_TABLE_START,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            Console.BufferHeight - TuiRenderer.TOP_TABLE_START - TuiRenderer.LOWER_TABLE_HEIGHT - 3,
            ConsoleColor.Black);

        RedrawOption(0, _currentOption == 0);
        RedrawOption(1, _currentOption == 1);
        RedrawOption(2, _currentOption == 2);
        RedrawDescription(_currentOption);

        DrawBottomInstructions();
    }

    public static void DrawMain(ConsoleKeyInfo newKeyStroke)
    {
        switch (newKeyStroke.Key)
        {
            case ConsoleKey.Escape:
                _wantsBootAbort = true;
                return;
            case ConsoleKey.Enter:
            {
                switch (_currentOption)
                {
                    case 1:
                        _osVersion = new OsVersion(0, 1, 0);
                        break;
                    case 2:
                    {
                        bool success = TryBootToFw();
                        if (!success)
                        {
                            Console.CursorTop = 6;
                            Console.CursorLeft = 1;

                            ConsoleColor previousColor = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to reboot to firmware UI!");
                            Console.ForegroundColor = previousColor;
                        }

                        break;
                    }
                }

                break;
            }
        }

        // unsafe
        // {
        //     ulong maxVarLength = 600;
        //     char* lastVarName = stackalloc char[300];
        //     lastVarName[0] = '\0';
        //
        //     EfiGuid lastGuid = new();
        //     Console.CursorTop = 7;
        //
        //     while (true)
        //     {
        //         maxVarLength = 600;
        //         EfiStatus status = Environment.EfiSysTable->RuntimeServices->GetNextVariableName(
        //             &maxVarLength,
        //             lastVarName,
        //             &lastGuid);
        //
        //         if (status == EfiStatus.NotFound)
        //         {
        //             Console.CursorTop = 6;
        //             Console.CursorLeft = 1;
        //             Console.WriteLine("ALL");
        //             break;
        //         }
        //
        //         if (status != EfiStatus.Success)
        //         {
        //             Console.CursorTop = 6;
        //             Console.CursorLeft = 1;
        //             Console.WriteLine(((uint)status).ToString());
        //             break;
        //         }
        //
        //         Console.CursorLeft = 1;
        //         Console.WriteRaw(lastVarName);
        //     }
        // }

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
        return _osVersion != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>
    /// Null if no navigation to settings was desired, a version indicating the OS version the user wants to customize.
    /// </returns>
    public static OsVersion? GetVersionForSettingsNavigation()
    {
        return _osVersion;
    }

    public static bool WantsBootAbort()
    {
        return _wantsBootAbort;
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
                const string OPTION_DESC_3_SUPPORTED =
                    "Reboot the system and enter into the device's firmware settings (not supported on all devices).";
                const string OPTION_DESC_3_UNSUPPORTED =
                    "Reboot the system and enter into the device's firmware settings (might not work on this device, " +
                    "so it might reboot in this same screen).";

                Console.Write(_supportsBootToFw ? OPTION_DESC_3_SUPPORTED : OPTION_DESC_3_UNSUPPORTED);
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

        //clear
        TuiRenderer.DrawRect(
            1,
            topLimit,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            TuiRenderer.LOWER_TABLE_HEIGHT,
            ConsoleColor.Black);

        Console.CursorLeft = 2;
        Console.WriteLine("\u2191 \u2193  [Up/down arrow] Navigate options");
        Console.CursorLeft = 2;
        Console.WriteLine("\u23ce  [Enter] Select option");
    }

    private static void DrawTitle()
    {
        TuiRenderer.DrawRect(0, 0, Console.BufferWidth, 2, ConsoleColor.DarkGreen);
        int col = TuiRenderer.AlignText(
            TuiRenderer.CHIHUAHUA_OS, 0, Console.BufferWidth, TuiRenderer.TextAlignment.Center);

        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.CursorLeft = col;
        Console.CursorTop = 1;
        Console.Write(TuiRenderer.CHIHUAHUA_OS);
    }

    private static unsafe void CheckSupportForBootToFw()
    {
        const string VAR_NAME_SUPPORTED = "OsIndicationsSupported";
        EfiGuid globalVariableGuid = AllEfiGuids.EfiGlobalVariable;

        fixed (char* pVar = VAR_NAME_SUPPORTED)
        {
            EfiVariableAttributes attributes;
            ulong osIndications;
            ulong dataSize = sizeof(ulong);

            EfiStatus status = Environment.EfiSysTable->RuntimeServices->GetVariable(
                pVar,
                &globalVariableGuid,
                &attributes,
                &dataSize,
                &osIndications);

            if (status != EfiStatus.Success)
            {
                _supportsBootToFw = false;
                return;
            }

            _supportsBootToFw = (osIndications & EFI_OS_INDICATIONS_BOOT_TO_FW_UI) != 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>
    /// Returns false only if the option is supported, but there was an error setting the variable. Otherwise, it will
    /// simply request a reboot immediately.
    /// </returns>
    private static unsafe bool TryBootToFw()
    {
        const string VAR_NAME_SUPPORTED = "OsIndications";
        EfiGuid globalVariableGuid = AllEfiGuids.EfiGlobalVariable;

        fixed (char* pVar = VAR_NAME_SUPPORTED)
        {
            const EfiVariableAttributes WRITE_ATTRIBUTES =
                EfiVariableAttributes.NonVolatile
                | EfiVariableAttributes.BootServiceAccess
                | EfiVariableAttributes.RuntimeAccess;

            EfiVariableAttributes readAttributes;
            ulong osIndications;
            ulong dataSize = sizeof(ulong);

            //first, we read the variable and try to modify only the relevant part; if we can't read the variable,
            // we simply set the raw value directly
            EfiStatus status = Environment.EfiSysTable->RuntimeServices->GetVariable(
                pVar,
                &globalVariableGuid,
                &readAttributes,
                &dataSize,
                &osIndications);
            if (status != EfiStatus.Success)
            {
                osIndications = EFI_OS_INDICATIONS_BOOT_TO_FW_UI;
            }
            else
            {
                osIndications |= EFI_OS_INDICATIONS_BOOT_TO_FW_UI;
            }

            status = Environment.EfiSysTable->RuntimeServices->SetVariable(
                pVar,
                &globalVariableGuid,
                WRITE_ATTRIBUTES,
                sizeof(ulong),
                &osIndications);

            //if the set was successful, we reboot
            if (status == EfiStatus.Success)
            {
                Environment.EfiSysTable->RuntimeServices->ResetSystem(
                    EfiResetType.EfiResetCold,
                    EfiStatus.Success,
                    0,
                    null);
                return true;
            }

            //otherwise, if we already know that the device supports this and we couldn't set the variable, we
            // return false (no action)
            if (_supportsBootToFw)
            {
                return false;
            }

            //finally, if we couldn't set, but we already know that we can't set it, we simply exit the bootloader
            _wantsBootAbort = true;
            return true;
        }
    }
}