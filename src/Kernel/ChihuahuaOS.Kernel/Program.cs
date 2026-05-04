using System.Runtime;

namespace ChihuahuaOS.Kernel;

internal static class Program
{
    [RuntimeExport("_start")]
    private static void _start()
    {
    }

    private static void Main(string[] args)
    {
    }
}