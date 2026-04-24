using System;
using ChihuahuaOS.CoreLib.Extra;
using ChihuahuaOS.EfiApi;
using ChihuahuaOS.EfiApi.RuntimeServices;

namespace ChihuahuaOS.Bootloader.Tui;

internal static class MainScreen
{
    private static int _numOptions;
    private static StaticRef<OsVersion[]> _availableOsVersions;

    private static int _currentOption;
    private static bool _wantsBootAbort;
    private static bool _supportsBootToFw;

    private static OsVersion? _settingsOsVersion;

    private const ulong EFI_OS_INDICATIONS_BOOT_TO_FW_UI = 0x01;

    public static void OnEnterScreen()
    {
        //TODO: actually check and list all OS versions
        _numOptions = 4;
        _availableOsVersions.GetValue()?.Dispose();
        OsVersion[] osVersions = [new(0, 1, 0)];
        _availableOsVersions.SetValue(osVersions);

        CheckSupportForBootToFw();
        DrawTitle();

        //reset
        _settingsOsVersion = null;
        _wantsBootAbort = false;

        //clear the main area
        TuiRenderer.DrawRect(
            1,
            TuiRenderer.TOP_TABLE_START,
            Console.BufferWidth - 2,
            //we extract the upper padding, the lower table height, then the 3 borders
            Console.BufferHeight - TuiRenderer.TOP_TABLE_START - TuiRenderer.LOWER_TABLE_HEIGHT - 3,
            ConsoleColor.Black);

        for (int i = 0; i < osVersions.Length; i++)
        {
            int idx = i * 2;
            RedrawOption(idx, _currentOption == idx);
            RedrawOption(idx + 1, _currentOption == idx + 1);
        }

        RedrawOption(_numOptions - 2, _currentOption == _numOptions - 2);
        RedrawOption(_numOptions - 1, _currentOption == _numOptions - 1);
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
                if (_currentOption < _numOptions - 2)
                {
                    if (_currentOption % 2 == 0)
                    {
                        //TODO: boot the OS   
                    }
                    else if (_availableOsVersions.HasValue())
                    {
                        _settingsOsVersion = _availableOsVersions.GetValue()![_currentOption / 2];
                    }
                }
                else if (_currentOption == _numOptions - 2)
                {
                    //TODO: add boot manager settings
                }
                else if (_currentOption == _numOptions - 1)
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
                }

                break;
            }
        }

        int previousOption = _currentOption;

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (newKeyStroke.Key == ConsoleKey.UpArrow && _currentOption > 0)
        {
            _currentOption--;
        }
        else if (newKeyStroke.Key == ConsoleKey.DownArrow && _currentOption < _numOptions)
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
        return _settingsOsVersion != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>
    /// Null if no navigation to settings was desired, a version indicating the OS version the user wants to customize.
    /// </returns>
    public static OsVersion? GetVersionForSettingsNavigation()
    {
        return _settingsOsVersion;
    }

    public static bool WantsBootAbort()
    {
        return _wantsBootAbort;
    }


    private static void RedrawOption(int index, bool isHighlighted)
    {
        if (!_availableOsVersions.HasValue())
        {
            return;
        }

        OsVersion[] osVersions = _availableOsVersions.GetValue()!;
        Console.CursorLeft = 1;
        Console.CursorTop = TuiRenderer.TOP_TABLE_START + index;
        int charsDrawn = 0;

        if (isHighlighted)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ->");
            charsDrawn += 3;
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        if (index < _numOptions - 2)
        {
            if (index % 2 == 0)
            {
                using string optionText = " Start ChihuahuaOS v." + osVersions[index / 2];
                Console.Write(optionText);
                charsDrawn += optionText.Length;
            }
            else
            {
                using string optionText =
                    " Enter OS boot settings (for v." + osVersions[index / 2] + ")";
                Console.Write(optionText);
                charsDrawn += optionText.Length;
            }
        }
        else if (index == _numOptions - 2)
        {
            const string OPTION_N_2 = " Enter Chihuahua boot manager settings";
            Console.Write(OPTION_N_2);
            charsDrawn += OPTION_N_2.Length;
        }
        else if (index == _numOptions - 1)
        {
            const string OPTION_N_1 = " Enter UEFI firmware settings";
            Console.Write(OPTION_N_1);
            charsDrawn += OPTION_N_1.Length;
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
        if (!_availableOsVersions.HasValue())
        {
            return;
        }

        OsVersion[] osVersions = _availableOsVersions.GetValue()!;

        //we extract the lower table height, then the 2 borders, then 3 rows for this description
        int startRow = Console.BufferHeight - 1 - TuiRenderer.LOWER_TABLE_HEIGHT - 2 - 3;
        CleanupDescription(startRow);

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorLeft = 1;
        Console.CursorTop = startRow;

        Console.Write(" INFO: ");
        if (index < _numOptions - 2)
        {
            if (index % 2 == 0)
            {
                using string optionDescText =
                    "Starts Chihuahua OS (version " + osVersions[index / 2] + ").";
                Console.Write(optionDescText);
            }
            else
            {
                using string optionDescText =
                    "Configure the boot settings for ChihuahuaOS (version "
                    + osVersions[index / 2]
                    + "), settings that might impact the mode the OS boots.";
                Console.Write(optionDescText);
            }
        }
        else if (index == _numOptions - 2)
        {
            const string OPTION_DESC_N_2 =
                "Configure the settings for this graphical boot manager settings.";
            Console.Write(OPTION_DESC_N_2);
        }
        else if (index == _numOptions - 1)
        {
            const string OPTION_DESC_N_1_SUPPORTED =
                "Reboot the system and enter into the device's firmware settings.";
            const string OPTION_DESC_N_1_UNSUPPORTED =
                "Reboot the system and enter into the device's firmware settings (might not work on this device, " +
                "so it might reboot in this same screen).";

            Console.Write(_supportsBootToFw ? OPTION_DESC_N_1_SUPPORTED : OPTION_DESC_N_1_UNSUPPORTED);
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
        Console.WriteLine("[Up/down arrow] Navigate options");
        Console.CursorLeft = 2;
        Console.WriteLine("[Enter] Select option");
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

            //otherwise, if we already know that the device supports this, and we couldn't set the variable, we
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