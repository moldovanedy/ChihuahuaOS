namespace System;

public struct Int16
{
    // ReSharper disable InconsistentNaming
    public const short MaxValue = 0x7FFF;

    public const short MinValue = unchecked((short)0x8000);
    // ReSharper restore InconsistentNaming
}