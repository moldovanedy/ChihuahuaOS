using System;
using System.Runtime.CompilerServices;
using ChihuahuaOS.BootParams;
using ChihuahuaOS.CoreLib;
using ChihuahuaOS.Fs.Ustar;
using ChihuahuaOS.Kernel.FramebufferManager.Psf;

namespace ChihuahuaOS.Kernel.FramebufferManager;

internal static unsafe class TextRenderer
{
    public static uint WidthInChars { get; private set; }
    public static uint HeightInChars { get; private set; }

    public static uint X { get; set; }
    public static uint Y { get; set; }

    private static PsfHandler _psfHandler;

    private static SolidColor _fgColor;
    private static SolidColor _bgColor;

    public static void Init()
    {
        UstarReaderNoAlloc rdReader = new(
            (byte*)KVirtualAddresses.INITRD_BASE,
            (long)Program.KernelParamsPtr->InitRdSize);
        byte* filePtr = rdReader.GetFilePointer("Assets/Fonts/Uni2_Terminus_8x16_n.psfu\0"u8, out long fileLength);
        if (filePtr == null || fileLength <= 0)
        {
            CoreLibManager.Panic(null);
        }

        _psfHandler = new PsfHandler(filePtr, fileLength);

        //+1 pixel per char to keep things readable
        WidthInChars = Framebuffer.Width / (_psfHandler.Header.Width + 1);
        HeightInChars = Framebuffer.Height / _psfHandler.Header.Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPosition(int x, int y)
    {
        X = (uint)Math.Max(0, Math.Min(WidthInChars - 1, x));
        Y = (uint)Math.Max(0, Math.Min(HeightInChars - 1, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetForegroundColor(SolidColor color)
    {
        _fgColor = color;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBackgroundColor(SolidColor color)
    {
        _bgColor = color;
    }

    public static void DrawText(ReadOnlySpan<byte> text)
    {
        DrawText((byte*)text);
    }

    public static void DrawText(byte* text)
    {
        int i = 0;
        while (text[i] != '\0')
        {
            byte character = text[i];
            if (character == '\n')
            {
                X = WidthInChars;
                AdvancePosition();
                i++;
                continue;
            }

            if (character == '\r')
            {
                X = 0;
                i++;
                continue;
            }

            DrawChar(character);

            //draw the padding (on the right)
            if (X >= WidthInChars - 1)
            {
                int xPos = (int)(X * (_psfHandler.Header.Width + 1));
                Framebuffer.DrawRect(
                    xPos,
                    (int)(Y * _psfHandler.Header.Height),
                    (int)Framebuffer.Width - xPos,
                    (int)_psfHandler.Header.Height,
                    _bgColor);
            }

            //draw the padding (on the bottom)
            if (Y == HeightInChars - 1)
            {
                int yPos = (int)(Y * _psfHandler.Header.Height);
                Framebuffer.DrawRect(
                    (int)(X * (_psfHandler.Header.Width + 1)),
                    yPos,
                    (int)_psfHandler.Header.Width + 1,
                    (int)Framebuffer.Height - yPos,
                    _bgColor);
            }

            AdvancePosition();
            i++;
        }
    }


    private static void DrawChar(byte character)
    {
        uint* fbBase = (uint*)KVirtualAddresses.GOP_BASE;
        fbBase += Y * _psfHandler.Header.Height * Program.KernelParamsPtr->FramebufferInfo->PixelsPerScanLine;
        fbBase += X * (_psfHandler.Header.Width + 1);

        byte* glyphDataPtr = _psfHandler.GetCharacterDataNoUnicode(character);
        uint bgColorRaw = Framebuffer.GetRawColor(_bgColor);
        uint fgColorRaw = Framebuffer.GetRawColor(_fgColor);
        int bytesPerRow = ((int)_psfHandler.Header.Width + 7) / 8;

        for (int i = 0; i < _psfHandler.Header.Height; i++)
        {
            int byteOffsetInRow = -1;
            for (int j = 0; j < _psfHandler.Header.Width; j++)
            {
                if (j % 8 == 0)
                {
                    byteOffsetInRow++;
                }

                byte localGlyphData = *(glyphDataPtr + i * bytesPerRow + byteOffsetInRow);
                bool isFilled = (localGlyphData & (1 << (7 - j % 8))) != 0;
                fbBase[0] = isFilled ? fgColorRaw : bgColorRaw;

                fbBase++;
            }

            //draw the BG color on the one px space
            fbBase[0] = bgColorRaw;

            fbBase += Program.KernelParamsPtr->FramebufferInfo->PixelsPerScanLine;
            fbBase -= _psfHandler.Header.Width;
        }
    }

    private static void AdvancePosition()
    {
        if (X + 1 < WidthInChars)
        {
            X++;
            return;
        }

        X = 0;

        if (Y + 1 < HeightInChars)
        {
            Y++;
            // return;
        }

        //TODO: check the max, advance y as well, and scroll if necessary
    }
}
