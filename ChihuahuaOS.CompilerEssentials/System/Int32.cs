namespace System;

public struct Int32
{
    // ReSharper disable InconsistentNaming
    public const int MaxValue = 0x7fffffff;

    public const int MinValue = unchecked((int)0x80000000);
    // ReSharper restore InconsistentNaming
}