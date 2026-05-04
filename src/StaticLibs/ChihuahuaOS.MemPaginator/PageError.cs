namespace ChihuahuaOS.MemPaginator;

public enum PageError
{
    Success = 0,
    EntryExists = 1,
    InvalidVirtualAddress = 2,
    OutOfMemory = 3,

    UnknownError = int.MaxValue
}