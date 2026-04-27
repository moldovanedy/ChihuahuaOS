using ChihuahuaOS.CoreLib;

namespace Internal.Runtime.CompilerHelpers;

public static unsafe class ThrowHelpers
{
    public static void ThrowArgumentException()
    {
        CoreLibManager.Panic("Argument exception".ToCharPtrUnsafe());
    }

    public static void ThrowInvalidOperationException()
    {
        CoreLibManager.Panic("Invalid operation exception".ToCharPtrUnsafe());
    }

    public static void ThrowInvalidCastException()
    {
        CoreLibManager.Panic("Invalid cast exception".ToCharPtrUnsafe());
    }

    public static void ThrowOverflowException()
    {
        CoreLibManager.Panic("Overflow exception".ToCharPtrUnsafe());
    }

    public static void ThrowFormatException()
    {
        CoreLibManager.Panic("Format exception".ToCharPtrUnsafe());
    }

    public static void ThrowArgumentOutOfRangeException()
    {
        CoreLibManager.Panic("Argument out of range exception".ToCharPtrUnsafe());
    }

    public static void ThrowNullReferenceException()
    {
        CoreLibManager.Panic("Null reference exception".ToCharPtrUnsafe());
    }

    public static void ThrowIndexOutOfRangeException()
    {
        CoreLibManager.Panic("Index out of range exception".ToCharPtrUnsafe());
    }

    public static void ThrowDivideByZeroException()
    {
        CoreLibManager.Panic("Divide by zero exception".ToCharPtrUnsafe());
    }

    public static void ThrowPlatformNotSupportedException()
    {
        CoreLibManager.Panic("Platform not supported exception".ToCharPtrUnsafe());
    }

    public static void ThrowNotImplementedException()
    {
        CoreLibManager.Panic("Not implemented exception".ToCharPtrUnsafe());
    }

    public static void ThrowInvalidProgramException()
    {
        CoreLibManager.Panic("Invalid program exception".ToCharPtrUnsafe());
    }

    public static void ThrowStreamException()
    {
        CoreLibManager.Panic("Stream exception".ToCharPtrUnsafe());
    }

    public static void ThrowInvalidProgramExceptionWithArgument()
    {
        CoreLibManager.Panic("Invalid program exception with argument".ToCharPtrUnsafe());
    }
}