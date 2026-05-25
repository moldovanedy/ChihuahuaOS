namespace System;

public struct Char
{
    public const char MaxValue = (char)0xFFFF;

    public const char MinValue = (char)0x00;


    public static bool TryParse(string s, out char result)
    {
        result = '\0';

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

        result = (char)parsed;
        return true;
    }


    public override string ToString()
    {
        return new string(this, 1);
    }
}