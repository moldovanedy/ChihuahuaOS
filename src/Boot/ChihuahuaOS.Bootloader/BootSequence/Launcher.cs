using System;
using System.Collections.Generic;
using System.IO;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.Bootloader.SettingsManager;
using ChihuahuaOS.MemPaginator;

namespace ChihuahuaOS.Bootloader.BootSequence;

public static class Launcher
{
    internal static KernelSettings KSettings;
    internal static OsVersion BootedOsVersion;

    public static void StartBoot(OsVersion osVersion)
    {
        //re-enable the watchdog to 60 seconds
        unsafe
        {
            if (Environment.EfiSysTable != null)
            {
                Environment.EfiSysTable->BootServices->SetWatchdogTimer(60, 0, 0, null);
            }
        }

        BootedOsVersion = osVersion;
        Console.Clear();
        Console.CursorLeft = 0;
        Console.CursorTop = 0;

        string bootUpMessage = "Start booting ChihuahuaOS version " + osVersion + "!";
        Console.WriteLine(bootUpMessage);
        bootUpMessage.Dispose();

        bool success = LoadKernelSettings();
        //not fatal, we can go on
        if (!success)
        {
            KSettings = new KernelSettings();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARN: Could not load kernel settings. Continuing with the default settings.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        success = GopSetter.SetAppropriateFramebuffer();
        // unsafe
        // {
        //     EfiSystemTable* st = Environment.EfiSysTable;
        //     st->ConOut->Reset(st->ConOut, false);
        // }

        if (success)
        {
            Console.WriteLine("Successfully set the display resolution.");
        }
        else
        {
            //not fatal, we can go on
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                "WARN: Could not change the display resolution according to the settings." +
                " Continuing with the current mode.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        MemMap.EfiMap? efiMapOpt = MemMap.GetMemoryMap();
        if (efiMapOpt == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                "FATAL ERROR: Could not retrieve the system memory map!");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        MemMap.EfiMap efiMap = efiMapOpt.Value;
        Console.WriteLine("Successfully retrieved the system memory map.");

        success = MemMap.SetupPagingStructures(efiMap, out PagingManager? _);
        //TEMP
        efiMap.Dispose();

        if (success)
        {
            Console.WriteLine("Successfully setup paging structures.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                "FATAL ERROR: Could not setup paging structures!");
            Console.ForegroundColor = ConsoleColor.White;
            Fail();
            return;
        }

        //TODO: boot

        Fail();
    }

    private static void Fail()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Boot failed! Press any key to return to the main menu...");
        Console.ForegroundColor = ConsoleColor.White;
        _ = Console.ReadKey();
    }

    private static bool LoadKernelSettings()
    {
        using string settingsFilePath = "\\EFI\\BOOT\\ChiOS_" + BootedOsVersion + ".CFG";
        using FileStream? fs = File.OpenRead(settingsFilePath);
        if (fs == null)
        {
            return false;
        }

        List<TomlSetting> settings = TomlManager.ReadFromStream(fs, KernelSettings.NUM_SETTINGS);
        KSettings = KernelSettings.FromConfigList(settings);
        settings.Dispose();
        return true;
    }
}