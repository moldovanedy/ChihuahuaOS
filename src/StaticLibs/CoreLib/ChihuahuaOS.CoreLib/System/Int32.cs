using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Int32
{
    public const int MaxValue = 0x7F_FF_FF_FF;

    public const int MinValue = unchecked((int)0x80000000);


    public static bool TryParse(string s, out int result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out long parsed);
        if (!success)
        {
            return false;
        }

        //check for overflow or underflow
        if (parsed > MaxValue || parsed < MinValue)
        {
            return false;
        }

        result = (int)parsed;
        return true;
    }


    public override string ToString()
    {
        return NumberParser.ParseInteger(this);
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
                return NumberParser.ParseInteger(this, 16);
            case "B":
                return NumberParser.ParseInteger(this, 2);
            default:
                ThrowHelpers.ThrowFormatException();
                return string.Empty;
        }
    }
}