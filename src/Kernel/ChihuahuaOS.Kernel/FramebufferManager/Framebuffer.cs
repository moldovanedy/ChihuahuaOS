using System;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.BootParams.ParamsData;

namespace ChihuahuaOS.Kernel.FramebufferManager;

public static unsafe class Framebuffer
{
    public static uint Width { get; private set; }
    public static uint Height { get; private set; }

    internal static FbInfo Info { get; private set; }

    internal static void Init()
    {
        Info = Program.KernelParamsPtr->FramebufferInfo;
        Width = Info.Width;
        Height = Info.Height;
    }

    public static void Clear(SolidColor color)
    {
        uint rawColor = GetRawColor(color);
        uint* gop = (uint*)Program.KernelParamsPtr->VirtualSpaceInfo.GopBase;
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                gop[i * Info.PixelsPerScanLine + j] = rawColor;
            }
        }
    }

    public static void DrawRect(int x, int y, int width, int height, SolidColor color)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        if (y < 0)
        {
            y = 0;
        }

        if (x < 0)
        {
            x = 0;
        }

        uint rawColor = GetRawColor(color);
        uint* gop = (uint*)Program.KernelParamsPtr->VirtualSpaceInfo.GopBase;
        for (int i = y; i < y + height; i++)
        {
            if (i >= Height)
            {
                break;
            }

            for (int j = x; j < x + width; j++)
            {
                if (j >= Width)
                {
                    break;
                }

                gop[i * Info.PixelsPerScanLine + j] = rawColor;
            }
        }
    }

    internal static uint GetRawColor(SolidColor color)
    {
        //NOTE: all these calculations are made only for little-endian

        const int NUM_BITS = 32;
        const int MAX_CAPACITY = 256;

        const uint RGBX32_RED_MASK = 0xFF_00_00_00;
        const uint RGBX32_GREEN_MASK = 0x00_FF_00_00;
        const uint RGBX32_BLUE_MASK = 0x00_00_FF_00;

        const uint BGRX32_RED_MASK = 0x00_00_FF_00;
        const uint BGRX32_GREEN_MASK = 0x00_FF_00_00;
        const uint BGRX32_BLUE_MASK = 0xFF_00_00_00;

        //fast route for RGBX32 or BGRX32
        if (Info.RedBitmask == RGBX32_RED_MASK
            && Info.GreenBitmask == RGBX32_GREEN_MASK
            && Info.BlueBitmask == RGBX32_BLUE_MASK)
        {
            return color.Red | (uint)(color.Green << 8) | (uint)(color.Blue << 16);
        }

        if (Info.RedBitmask == BGRX32_RED_MASK
            && Info.GreenBitmask == BGRX32_GREEN_MASK
            && Info.BlueBitmask == BGRX32_BLUE_MASK)
        {
            return (uint)(color.Red << 16) | (uint)(color.Green << 8) | color.Blue;
        }

        uint redCapacity = 1;
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (((Info.RedBitmask >> i) & 1) == 1)
            {
                redCapacity <<= 1;
            }
        }

        uint greenCapacity = 1;
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (((Info.GreenBitmask >> i) & 1) == 1)
            {
                greenCapacity <<= 1;
            }
        }

        uint blueCapacity = 1;
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (((Info.BlueBitmask >> i) & 1) == 1)
            {
                blueCapacity <<= 1;
            }
        }

        uint result = 0;

        uint divider = MAX_CAPACITY / redCapacity;
        uint redComponent = color.Red / divider;
        //midpoint rounding
        if (color.Red % divider > divider / 2)
        {
            redComponent++;
        }

        divider = MAX_CAPACITY / greenCapacity;
        uint greenComponent = color.Green / divider;
        if (color.Green % divider > divider / 2)
        {
            greenComponent++;
        }

        divider = MAX_CAPACITY / blueCapacity;
        uint blueComponent = color.Blue / divider;
        if (color.Blue % divider > divider / 2)
        {
            blueComponent++;
        }

        redComponent = Math.Min(redComponent, 255);
        greenComponent = Math.Min(greenComponent, 255);
        blueComponent = Math.Min(blueComponent, 255);

        int clearedRedBits = 0;
        int clearedGreenBits = 0;
        int clearedBlueBits = 0;
        for (int i = 0; i < NUM_BITS; i++)
        {
            if (((Info.RedBitmask >> i) & 1) == 1)
            {
                result |= ((redComponent >> clearedRedBits) & 1) << (NUM_BITS - i);
                clearedRedBits++;
            }

            if (((Info.GreenBitmask >> i) & 1) == 1)
            {
                result |= ((greenComponent >> clearedGreenBits) & 1) << (NUM_BITS - i);
                clearedGreenBits++;
            }

            if (((Info.BlueBitmask >> i) & 1) == 1)
            {
                result |= ((blueComponent >> clearedBlueBits) & 1) << (NUM_BITS - i);
                clearedBlueBits++;
            }
        }

        return result;
    }
}
