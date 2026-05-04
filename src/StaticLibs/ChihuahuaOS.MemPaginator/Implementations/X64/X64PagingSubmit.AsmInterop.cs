using System.Runtime.InteropServices;

#if ARCH_X64
namespace ChihuahuaOS.MemPaginator.Implementations.X64;

public static class X64PagingSubmit
{
    public static void SubmitPageTable(ulong physicalAddress)
    {
        Paging_SubmitPageTable(physicalAddress);
    }

    public static void InvalidatePage(ulong virtualAddress)
    {
        Paging_InvalidatePage(virtualAddress);
    }

    [DllImport("*")]
    private static extern void Paging_SubmitPageTable(ulong physicalAddress);

    [DllImport("*")]
    private static extern void Paging_InvalidatePage(ulong virtualAddress);
}

#endif