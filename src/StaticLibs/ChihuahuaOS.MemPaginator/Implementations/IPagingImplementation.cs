namespace ChihuahuaOS.MemPaginator.Implementations;

internal interface IPagingImplementation
{
    PageError MapPage(ulong physicalAddress, ulong virtualAddress, PageFlags flags);

    void UnmapPage(ulong virtualAddress);

    PageError SubmitChanges();

    ulong GetRootPageTablePhysicalAddress();

    ulong VirtualToPhysical(ulong virtualAddress);

    ulong PhysicalToVirtual(ulong physicalAddress);
}