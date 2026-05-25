using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.Kernel.FramebufferManager;

namespace ChihuahuaOS.Kernel;

internal static unsafe class ConsoleManager
{
    public static void Init()
    {
        Console.SetupHandlers(
            &ChangeConsoleBgColor,
            &ChangeConsoleFgColor,
            &RequestBufferSize,
            &RequestCursorPos,
            &SetCursorPos,
            &SetCursorVisibility,
            &Print,
            &FastClear);

        TextRenderer.SetBackgroundColor(new SolidColor(0x00_00_00));
        TextRenderer.SetForegroundColor(new SolidColor(0xFF_FF_FF));
    }

    [UnmanagedCallersOnly]
    private static void ChangeConsoleFgColor(ConsoleColor fgColor)
    {
        TextRenderer.SetForegroundColor(GetStandardConsoleColor(fgColor));
    }

    [UnmanagedCallersOnly]
    private static void ChangeConsoleBgColor(ConsoleColor bgColor)
    {
        TextRenderer.SetBackgroundColor(GetStandardConsoleColor(bgColor));
    }

    [UnmanagedCallersOnly]
    private static (uint, uint) RequestBufferSize()
    {
        return (TextRenderer.WidthInChars, TextRenderer.HeightInChars);
    }

    [UnmanagedCallersOnly]
    private static (uint, uint) RequestCursorPos()
    {
        return (TextRenderer.X, TextRenderer.Y);
    }

    [UnmanagedCallersOnly]
    private static void SetCursorPos(uint x, uint y)
    {
        TextRenderer.X = Math.Min(x, TextRenderer.WidthInChars - 1);
        TextRenderer.Y = Math.Min(y, TextRenderer.HeightInChars - 1);
    }

    [UnmanagedCallersOnly]
    private static void SetCursorVisibility(bool isVisibleNow)
    {
    }

    [UnmanagedCallersOnly]
    private static void Print(byte* text)
    {
        TextRenderer.DrawText(text);
    }

    [UnmanagedCallersOnly]
    private static void FastClear()
    {
        Framebuffer.Clear(GetStandardConsoleColor(Console.BackgroundColor));
    }


    private static SolidColor GetStandardConsoleColor(ConsoleColor color)
    {
        switch (color)
        {
            default:
            case ConsoleColor.Black:
                return new SolidColor(0);
            case ConsoleColor.DarkBlue:
                return new SolidColor(0x00_00_AA);
            case ConsoleColor.DarkGreen:
                return new SolidColor(0x00_AA_00);
            case ConsoleColor.DarkCyan:
                return new SolidColor(0x00_AA_AA);
            case ConsoleColor.DarkRed:
                return new SolidColor(0xAA_00_00);
            case ConsoleColor.DarkMagenta:
                return new SolidColor(0xAA_00_AA);
            case ConsoleColor.DarkYellow: //VGA brown
                return new SolidColor(0xAA_55_00);
            case ConsoleColor.DarkGray: //VGA white
                return new SolidColor(0xAA_AA_AA);
            case ConsoleColor.Gray:
                return new SolidColor(0x55_55_55);
            case ConsoleColor.Blue:
                return new SolidColor(0x55_55_FF);
            case ConsoleColor.Green:
                return new SolidColor(0x55_FF_55);
            case ConsoleColor.Cyan:
                return new SolidColor(0x55_FF_FF);
            case ConsoleColor.Red:
                return new SolidColor(0xFF_55_55);
            case ConsoleColor.Magenta:
                return new SolidColor(0xFF_55_FF);
            case ConsoleColor.Yellow:
                return new SolidColor(0xFF_FF_55);
            case ConsoleColor.White:
                return new SolidColor(0xFF_FF_FF);
        }
    }
}
