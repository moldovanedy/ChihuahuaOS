using System;

namespace ChihuahuaOS.MemPaginator;

[Flags]
public enum PageFlags
{
    None = 0,

    /// <summary>
    /// The page is present in RAM.
    /// </summary>
    Present = 1,

    /// <summary>
    /// Data is accessible by both the kernel and user-space.
    /// </summary>
    UserSpaceAccessible = 1 << 1,

    /// <summary>
    /// Data can be executed.
    /// </summary>
    ExecutePermission = 1 << 2,

    /// <summary>
    /// Data can be written.
    /// </summary>
    WritePermission = 1 << 3,

    /// <summary>
    /// Data can be read.
    /// </summary>
    ReadPermission = 1 << 4,

    /// <summary>
    /// This is a huge page, generally 2 MiB instead of the usual 4 KiB.
    /// </summary>
    IsHugePage = 1 << 15
}