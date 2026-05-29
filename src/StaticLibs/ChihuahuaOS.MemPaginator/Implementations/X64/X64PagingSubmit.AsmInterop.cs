#if ARCH_X64
using System.Runtime.InteropServices;

namespace ChihuahuaOS.MemPaginator.Implementations.X64;

public static class X64PagingSubmit
{
    public static void SubmitPageTable(ulong physicalAddress)
    {
#if UEFI
        Paging_SubmitPageTable__UefiAbi(physicalAddress);
#else
        Paging_SubmitPageTable__SysVAbi(physicalAddress);
#endif
    }

    public static void InvalidatePage(ulong virtualAddress)
    {
#if UEFI
        Paging_InvalidatePage__UefiAbi(virtualAddress);
#else
        Paging_InvalidatePage__SysVAbi(virtualAddress);
#endif
    }

    public static ulong GetRootPageTable()
    {
        return Paging_GetRootPageTable() & 0xFFFF_FFFF_FFFF_F000;
    }


#if UEFI
    [DllImport("*")]
    private static extern void Paging_SubmitPageTable__UefiAbi(ulong physicalAddress);

    [DllImport("*")]
    private static extern void Paging_InvalidatePage__UefiAbi(ulong virtualAddress);

#else

    [DllImport("*")]
    private static extern void Paging_SubmitPageTable__SysVAbi(ulong physicalAddress);

    [DllImport("*")]
    private static extern void Paging_InvalidatePage__SysVAbi(ulong virtualAddress);
#endif

    [DllImport("*")]
    private static extern ulong Paging_GetRootPageTable();
}

#endif
