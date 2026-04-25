using Internal.Runtime.CompilerHelpers;

namespace System.Runtime;

//for some reason it needs this function and this class specifically (first found when concatenating a string object
// and a string literal with "+").
public static class TypeCast
{
    internal static void StelemRef(Array array, nint index, object? obj)
    {
        StartupCodeHelpers.StelemRef(array, index, obj);
    }
}