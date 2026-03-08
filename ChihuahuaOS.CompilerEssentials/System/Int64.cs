namespace System;

public struct Int64
{
    // ReSharper disable InconsistentNaming
    public const long MaxValue = 0x7fffffffffffffffL;

    public const long MinValue = unchecked((long)0x8000000000000000L);
    // ReSharper restore InconsistentNaming
}