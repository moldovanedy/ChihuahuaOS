using ChihuahuaOS.CoreLib;

namespace Internal.Runtime.CompilerHelpers;

public static unsafe class ThrowHelpers
{
    public static void ThrowArgumentException()
    {
#if UEFI
        CoreLibManager.Panic("Argument exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Argument exception\0"u8);
#endif
    }

    public static void ThrowInvalidOperationException()
    {
#if UEFI
        CoreLibManager.Panic("Invalid operation exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Invalid operation exception\0"u8);
#endif
    }

    public static void ThrowInvalidCastException()
    {
#if UEFI
        CoreLibManager.Panic("Invalid cast exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Invalid cast exception\0"u8);
#endif
    }

    public static void ThrowOverflowException()
    {
#if UEFI
        CoreLibManager.Panic("Overflow exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Overflow exception\0"u8);
#endif
    }

    public static void ThrowFormatException()
    {
#if UEFI
        CoreLibManager.Panic("Format exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Format exception\0"u8);
#endif
    }

    public static void ThrowArgumentOutOfRangeException()
    {
#if UEFI
        CoreLibManager.Panic("Argument out of range exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Argument out of range\0"u8);
#endif
    }

    public static void ThrowNullReferenceException()
    {
#if UEFI
        CoreLibManager.Panic("Null reference exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Null reference exception\0"u8);
#endif
    }

    public static void ThrowIndexOutOfRangeException()
    {
#if UEFI
        CoreLibManager.Panic("Index out of range exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Index out of range\0"u8);
#endif
    }

    public static void ThrowDivideByZeroException()
    {
#if UEFI
        CoreLibManager.Panic("Divide by zero exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Divide by zero exception\0"u8);
#endif
    }

    public static void ThrowPlatformNotSupportedException()
    {
#if UEFI
        CoreLibManager.Panic("Platform not supported exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Platform not supported exception\0"u8);
#endif
    }

    public static void ThrowNotImplementedException()
    {
#if UEFI
        CoreLibManager.Panic("Not implemented exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Not implemented exception\0"u8);
#endif
    }

    public static void ThrowInvalidProgramException()
    {
#if UEFI
        CoreLibManager.Panic("Invalid program exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Invalid program exception\0"u8);
#endif
    }

    public static void ThrowStreamException()
    {
#if UEFI
        CoreLibManager.Panic("Stream exception".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Stream exception\0"u8);
#endif
    }

    public static void ThrowInvalidProgramExceptionWithArgument()
    {
#if UEFI
        CoreLibManager.Panic("Invalid program exception with argument".ToCharPtrUnsafe());
#else
        CoreLibManager.Panic((byte*)"Invalid program exception with argument\0"u8);
#endif
    }
}
