using System;

namespace Internal.Runtime.CompilerHelpers;

internal static class ThrowHelpers
{
    public static void ThrowArgumentException()
    {
        Environment.FailFast("Argument exception");
    }

    public static void ThrowInvalidOperationException()
    {
        Environment.FailFast("Invalid operation exception");
    }

    public static void ThrowInvalidCastException()
    {
        Environment.FailFast("Invalid cast exception");
    }

    public static void ThrowOverflowException()
    {
        Environment.FailFast("Overflow exception");
    }

    public static void ThrowFormatException()
    {
        Environment.FailFast("Format exception");
    }

    public static void ThrowArgumentOutOfRangeException()
    {
        Environment.FailFast("Argument out of range exception");
    }

    public static void ThrowNullReferenceException()
    {
        Environment.FailFast("Null reference exception");
    }

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

    public static void ThrowNotImplementedException()
    {
        Environment.FailFast("Not implemented exception");
    }

    public static void ThrowInvalidProgramException()
    {
        Environment.FailFast("Invalid program exception");
    }

    public static void ThrowStreamException()
    {
        Environment.FailFast("Stream exception");
    }

    public static void ThrowInvalidProgramExceptionWithArgument()
    {
        Environment.FailFast("Invalid program exception with argument");
    }
}