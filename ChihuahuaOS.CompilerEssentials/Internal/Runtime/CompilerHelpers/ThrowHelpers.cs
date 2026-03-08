using System;

namespace Internal.Runtime.CompilerHelpers;

public static class ThrowHelpers
{
    public static void ThrowIndexOutOfRangeException()
    {
        Environment.FailFast("Index out of range exception");
    }

    public static void ThrowDivideByZeroException()
    {
        Environment.FailFast("Divide by zero exception");
    }

    public static void ThrowPlatformNotSupportedException()
    {
        Environment.FailFast("Platform not supported exception");
    }
}