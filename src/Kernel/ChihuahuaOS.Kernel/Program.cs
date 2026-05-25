using System;
using System.Runtime.InteropServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Kernel.FramebufferManager;
using ChihuahuaOS.MinimalUtils;

namespace ChihuahuaOS.Kernel;

internal static unsafe class Program
{
    internal static KParams* KernelParamsPtr { get; set; }

    internal static void Main()
    {
        CoreLibManager.Panic = &Panic;
        CoreLibManager.PrimitiveDebug = &PrimitiveDebug;

        Framebuffer.Init();
        TextRenderer.Init();
        ConsoleManager.Init();

        //clear old screen
        Framebuffer.Clear(new SolidColor(0x00_00_00));
        Console.WriteLine("Welcome to ChihuahuaOS!\0"u8);

        SpinLocks.HaltingInfiniteLoop();
    }


    [UnmanagedCallersOnly]
    private static void Panic(byte* message)
    {
        Framebuffer.Clear(new SolidColor(0xAA_00_00));

        Console.CursorLeft = 0;
        Console.CursorTop = 0;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("KERNEL PANIC: \0"u8);
        Console.WriteRaw(message);

        SpinLocks.HaltingInfiniteLoop();
    }

    [UnmanagedCallersOnly]
    private static void PrimitiveDebug(byte* message)
    {
        TextRenderer.DrawText(message);
    }
}
