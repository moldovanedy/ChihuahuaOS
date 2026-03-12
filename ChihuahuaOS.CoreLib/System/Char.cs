namespace System;

public struct Char
{
    // ReSharper disable InconsistentNaming
    public const char MaxValue = (char)0xFFFF;

    public const char MinValue = (char)0x00;
    // ReSharper restore InconsistentNaming

    public override string ToString()
    {
        return new string(this, 1);
    }
}