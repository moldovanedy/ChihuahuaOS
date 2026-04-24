using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Byte
{
    public const byte MaxValue = 0xFF;

    public const byte MinValue = 0;


    public static bool TryParse(string s, out byte result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out ulong parsed);
        if (!success)
        {
            return false;
        }

        //check for overflow
        if (parsed > MaxValue)
        {
            return false;
        }

        result = (byte)parsed;
        return true;
    }


    public override string ToString()
    {
        return NumberParser.ParseInteger((ulong)this);
    }

    public string ToString(string format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return ToString();
        }

        switch (format)
        {
            case "X":
                return NumberParser.ParseInteger((ulong)this, 16);
            case "B":
                return NumberParser.ParseInteger((ulong)this, 2);
            default:
                ThrowHelpers.ThrowFormatException();
                return string.Empty;
        }
    }
}