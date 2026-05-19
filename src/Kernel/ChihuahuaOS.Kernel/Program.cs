using ChihuahuaOS.BootParams;
using ChihuahuaOS.Kernel.FramebufferManager;
using ChihuahuaOS.MinimalUtils;

namespace ChihuahuaOS.Kernel;

internal static unsafe class Program
{
    internal static KParams* KernelParamsPtr { get; set; }

    internal static void Main()
    {
        Framebuffer.Init();

        //clear
        Framebuffer.Clear(new SolidColor(0x00_00_00));

        SpinLocks.HaltingInfiniteLoop();
    }
}