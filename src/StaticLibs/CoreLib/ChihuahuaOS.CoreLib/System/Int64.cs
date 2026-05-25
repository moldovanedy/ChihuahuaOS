using Internal.Runtime.CompilerHelpers;

namespace System;

public struct Int64
{
    public const long MaxValue = 0x7F_FF_FF_FF_FF_FF_FF_FFL;

    public const long MinValue = unchecked((long)0x8000000000000000L);


    public static bool TryParse(string s, out long result)
    {
        result = 0;

        bool success = NumberParser.TryParseString(s, out long parsed);
        if (!success)
        {
            return false;
        }

        result = parsed;
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