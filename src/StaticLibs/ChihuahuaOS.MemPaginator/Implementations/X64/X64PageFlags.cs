#if ARCH_X64
using System;

namespace ChihuahuaOS.MemPaginator.Implementations.X64;

[Flags]
public enum X64PageFlags : ulong
{
    None = 0,
    Present = 1,

    /**
     * If set, the page is read/write, otherwise it's read-only.
     */
    WriteEnable = 1 << 1,

    /**
     * If set, this can be accessed by both user-mode and kernel-mode, otherwise just kernel-mode accessible.
     */
    UserSpaceAccessible = 1 << 2,
    WriteThrough = 1 << 3,
    CacheDisable = 1 << 4,
    Accessed = 1 << 5,

    /**
     * CPU writes this bit when writing to the mapped frame.
     */
    Dirty = 1 << 6,

    /**
     * If set, it means the page is a "huge" page of 2 MiB (on level 2) or 1 GiB (on level 3), otherwise it's
     * a normal, 4 KiB page.
     */
    HugePage = 1 << 7,

    /**
     * If set, it means the entry is available in all address spaces, so it isn't flushed by the TLB on an
     * address space switch.
     */
    Global = 1 << 8,

    /**
     * If set, this page is not executable; otherwise it can host executable code.
     */
    ExecuteDisable = 1UL << 63
}

#endif