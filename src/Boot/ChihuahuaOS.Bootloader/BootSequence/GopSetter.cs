using System;
using ChihuahuaOS.Bootloader.EfiInteractions;
using ChihuahuaOS.EfiApi.ConsoleSupport;

namespace ChihuahuaOS.Bootloader.BootSequence;

internal static class GopSetter
{
    public static bool SetAppropriateFramebuffer()
    {
        Gop.GopModeInfoEnumerator? gopEnumeratorOpt = Gop.GetModeInfoEnumerator();
        if (!gopEnumeratorOpt.HasValue)
        {
            return false;
        }

        if (Launcher.KSettings.ScreenWidth == 0)
        {
            Launcher.KSettings.ScreenWidth = 1920;
        }

        if (Launcher.KSettings.ScreenHeight == 0)
        {
            Launcher.KSettings.ScreenHeight = 1080;
        }

        Gop.GopModeInfoEnumerator gopEnumerator = gopEnumeratorOpt.Value;
        uint bestModeIndex;

        uint setModeIndex;
        EfiGopModeInformation setMode;
        unsafe
        {
            EfiGopMode? currMode = Gop.GetCurrentMode();
            if (currMode == null)
            {
                return false;
            }

            bestModeIndex = currMode.Value.Mode;
            setModeIndex = bestModeIndex;
            setMode = *currMode.Value.Info;
        }

        (int WDeviation, int HDeviation) bestDeviation = (
            (int)Launcher.KSettings.ScreenWidth - (int)setMode.HorizontalResolution,
            (int)Launcher.KSettings.ScreenHeight - (int)setMode.VerticalResolution);

        uint i = 0;
        while (gopEnumerator.MoveNext())
        {
            if (gopEnumerator.Current.PixelFormat == EfiGraphicsPixelFormat.PixelBltOnly)
            {
                i++;
                continue;
            }

            (int WDeviation, int HDeviation) thisDeviation = (
                (int)Launcher.KSettings.ScreenWidth - (int)gopEnumerator.Current.HorizontalResolution,
                (int)Launcher.KSettings.ScreenHeight - (int)gopEnumerator.Current.VerticalResolution);

            uint thisDeviationTotal = (uint)Math.Abs(thisDeviation.HDeviation + thisDeviation.WDeviation);
            uint bestDeviationTotal = (uint)Math.Abs(bestDeviation.HDeviation + bestDeviation.WDeviation);

            if (thisDeviationTotal < bestDeviationTotal)
            {
                bestDeviation = thisDeviation;
                bestModeIndex = i;
            }

            i++;
        }

        if (bestModeIndex == setModeIndex)
        {
            return true;
        }

        return Gop.SetMode(bestModeIndex);
    }
}