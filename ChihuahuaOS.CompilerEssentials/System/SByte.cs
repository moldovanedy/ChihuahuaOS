namespace System;

public struct SByte
{
    // ReSharper disable InconsistentNaming

    // The maximum value that a Byte may represent: 127.
    public const sbyte MaxValue = 0x7F;

    // The minimum value that a Byte may represent: -128.
    public const sbyte MinValue = unchecked((sbyte)0x80);

    // ReSharper restore InconsistentNaming
}