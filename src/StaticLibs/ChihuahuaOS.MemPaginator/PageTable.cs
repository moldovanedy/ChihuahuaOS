using System.Runtime.InteropServices;

namespace ChihuahuaOS.MemPaginator;

[StructLayout(LayoutKind.Sequential, Size = (int)PagingManager.PAGE_TABLE_SIZE)]
public unsafe ref struct PageTable
{
    public fixed ulong Entries[512];
}