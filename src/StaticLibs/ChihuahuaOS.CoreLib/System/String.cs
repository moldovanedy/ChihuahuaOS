using System.Runtime;
using Internal.Runtime;
using System.Runtime.CompilerServices;
using ChihuahuaOS.CoreLib.Extra.Runtime;
using Internal.Runtime.CompilerHelpers;

namespace System;

public sealed class String : IDisposable
{
    //internal ordering! DO NOT modify; also that's how it's supposed to have the auto-property
#pragma warning disable CS0649 // Field is never assigned to
    //intrinsic, no need to assign
    //the MSB (sign bit) is 1 for dynamically allocated string, 0 for constant strings
    private int _stringLength;
#pragma warning restore CS0649 // Field  never assigned to

    private char _firstChar;

    #region Properties

    public int Length => (int)((uint)_stringLength & ~(1 << 31));

#pragma warning disable CS8618 // Field must contain a non-null value
    // ReSharper disable once UnassignedReadonlyField
    [Intrinsic] public static readonly string Empty;
#pragma warning restore CS8618 // Field must contain a non-null value

    [IndexerName("Chars")]
    public char this[int index]
    {
        [Intrinsic] get => Unsafe.Add(ref _firstChar, index);
    }

    #endregion


    #region Ctors

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(char* value);

    internal static unsafe string Ctor(char* ptr)
    {
        char* cursor = ptr;
        while (*cursor++ != 0)
        {
        }

        int length = (int)(cursor - ptr - 1);
        string result = FastNewString(length);
        CopyCharsNullTerminating(result, ptr, length);

        return result;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char[]? value);

    internal static string Ctor(char[]? value)
    {
        if (value == null || value.Length == 0)
        {
            return Empty;
        }

        string result = FastNewString(value.Length);
        CopyCharsNullTerminating(result, value);

        return result;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(char* value, int startIndex, int length);

    internal static unsafe string Ctor(char* ptr, int startIndex, int length)
    {
        if (startIndex < 0 || length < 0)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
            return Empty;
        }

        if (ptr == null)
        {
            ThrowHelpers.ThrowArgumentException();
            return Empty;
        }

        char* pStart = ptr + startIndex;
        string result = FastNewString(length);
        CopyCharsNullTerminating(result, pStart, length);

        return result;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char[]? value, int startIndex, int length);

    internal static string Ctor(char[]? value, int startIndex, int length)
    {
        if (value == null)
        {
            ThrowHelpers.ThrowArgumentException();
            return Empty;
        }

        if (startIndex < 0 || length < 0 || startIndex > value.Length - length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
            return Empty;
        }

        string result = FastNewString(length);
        for (int i = 0; i < length; i++)
        {
            Unsafe.Add(ref result._firstChar, i) = value[startIndex + i];
        }

        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref result._firstChar, length) = '\0';

        return result;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern String(char c, int count);

    internal static string Ctor(char c, int count)
    {
        if (count < 0)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
            return Empty;
        }

        string result = FastNewString(count);
        for (int i = 0; i < count; i++)
        {
            Unsafe.Add(ref result._firstChar, i) = c;
        }

        //this is safe, as we always allocate one more char
        Unsafe.Add(ref result._firstChar, count) = '\0';

        return result;
    }

    #endregion


    #region Methods

    public override bool Equals(object? obj)
    {
        if (obj is string strObject)
        {
            return strObject.Length == Length && EqualsHelper(this, strObject);
        }

        return false;
    }

    public override int GetHashCode()
    {
        //for readonly field warning: we can safely suppress it, as the only time we actually modify _stringLength is
        // when we internally create a new one, therefore making it impossible to set the field as readonly

        // ReSharper disable once UsageOfDefaultStructEquality
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return _stringLength.GetHashCode();
    }

    public override string ToString()
    {
        return this;
    }

    /// <summary>
    /// Unlike in regular .NET, this actually creates a completely new string with the same contents.
    /// </summary>
    /// <returns></returns>
    public string Clone()
    {
        string result = FastNewString(Length);
        for (int i = 0; i < Length; i++)
        {
            Unsafe.Add(ref result._firstChar, i) = this[i];
        }

        //this is safe, as we always allocate one more char
        Unsafe.Add(ref result._firstChar, Length) = '\0';

        return result;
    }

    public bool Contains(char c)
    {
        return IndexOf(c) != -1;
    }

    public bool EndsWith(char value)
    {
        if (value != '\0')
        {
            // dereference Length now to front-load the null check; also take this time to zero-extend
            // n.b. (localLength - 1) could be negative!
            nuint localLength = (uint)Length;
            return Unsafe.Add(ref _firstChar, (nint)localLength - 1) == value;
        }

        int lastPos = Length - 1;
        return (uint)lastPos < (uint)Length && this[lastPos] == value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(char c)
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        return IndexOf(c, 0);
    }

    public int IndexOf(char c, int startIndex)
    {
        for (int i = startIndex; i < Length; i++)
        {
            char chr = this[i];
            if (chr == c)
            {
                return i;
            }
        }

        return -1;
    }

    public bool StartsWith(char value)
    {
        if (value != '\0')
        {
            return _firstChar == value;
        }

        return Length != 0 && _firstChar == value;
    }

    public string Substring(int startIndex, int length)
    {
        //zero-extending
        if ((ulong)(uint)startIndex + (uint)length > (uint)Length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        if (length == 0)
        {
            return Empty;
        }

        if (length == Length)
        {
            if (startIndex != 0)
            {
                ThrowHelpers.ThrowArgumentOutOfRangeException();
            }

            return this;
        }

        return InternalSubString(startIndex, length);
    }

    /// <summary>
    /// Returns the underlying char pointer. Is unsafe because it only temporarily fixes the memory, so any
    /// modification of the string might invalidate the pointer. This should only be used on const strings
    /// (or literals).
    /// </summary>
    /// <returns></returns>
    public unsafe char* ToCharPtrUnsafe()
    {
        fixed (char* ptr = this)
        {
            return ptr;
        }
    }

    public string ToLowerInvariant()
    {
        char[] chars = new char[Length];
        for (int i = 0; i < Length; i++)
        {
            if (this[i] >= 'A' && this[i] <= 'Z')
            {
                chars[i] = (char)(this[i] + 32);
            }
            else
            {
                chars[i] = this[i];
            }
        }

        string newString = new(chars);
        chars.Dispose();
        return newString;
    }

    public string ToUpperInvariant()
    {
        char[] chars = new char[Length];
        for (int i = 0; i < Length; i++)
        {
            if (this[i] >= 'a' && this[i] <= 'z')
            {
                chars[i] = (char)(this[i] - 32);
            }
            else
            {
                chars[i] = this[i];
            }
        }

        string newString = new(chars);
        chars.Dispose();
        return newString;
    }

    #endregion


    #region Static methods

    public static string Concat(string? str0, string? str1)
    {
        string?[] args = [str0, str1];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(string? str0, string? str1, string? str2)
    {
        string?[] args = [str0, str1, str2];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(string? str0, string? str1, string? str2, string? str3)
    {
        string?[] args = [str0, str1, str2, str3];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(object? obj0, object? obj1)
    {
        object?[] args = [obj0, obj1];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(object? obj0, object? obj1, object? obj2)
    {
        object?[] args = [obj0, obj1, obj2];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(object? obj0, object? obj1, object? obj2, object? obj3)
    {
        object?[] args = [obj0, obj1, obj2, obj3];
        string finalString = Concat(args);
        args.Dispose();
        return finalString;
    }

    public static string Concat(params object?[] args)
    {
        if (args.Length <= 1)
        {
            return args.Length == 0 ? Empty : args[0]?.ToString() ?? Empty;
        }

        string[] stringRepresentations = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            stringRepresentations[i] = args[i]?.ToString() ?? Empty;
        }

        string result = Concat(stringRepresentations);
        for (int i = 0; i < args.Length; i++)
        {
            stringRepresentations[i].Dispose();
        }

        return result;
    }

    public static string Concat(params string?[] args)
    {
        if (args.Length <= 1)
        {
            return args.Length == 0 ? Empty : args[0] ?? Empty;
        }

        int totalLength = 0;
        for (int i = 0; i < args.Length; i++)
        {
            totalLength += args[i]?.Length ?? 0;

            //positive overflow
            if (totalLength < 0)
            {
                ThrowHelpers.ThrowOverflowException();
            }
        }

        if (totalLength == 0)
        {
            return Empty;
        }

        string bigString = FastNewString(totalLength);
        int pos = 0;

        for (int i = 0; i < args.Length; i++)
        {
            string? s = args[i];
            if (s == null)
            {
                continue;
            }

            CopyStringContent(bigString, pos, s);
            pos += s.Length;
        }

        return bigString;
    }

    // public static bool Equals(string? a, string? b)
    // {
    //     if (a is null || b is null || a.Length != b.Length)
    //     {
    //         return false;
    //     }
    //
    //     return EqualsHelper(a, b);
    // }

    public static bool IsNullOrEmpty(string? value)
    {
        //(it IS the implementation)
        // ReSharper disable once ReplaceWithStringIsNullOrEmpty
        return value == null || value.Length == 0;
    }

    public static bool IsNullOrWhiteSpace(string? value)
    {
        ThrowHelpers.ThrowNotImplementedException();
        return value == null;
    }

    #endregion

    #region Operators

    public static bool operator ==(string? a, string? b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(string? a, string? b)
    {
        return !Equals(a, b);
    }

    #endregion

    #region Interface implementation

    public void Dispose()
    {
        if ((_stringLength & (1 << 31)) != 0)
        {
            MemUtils.FreeMemory(this);
        }
    }

    #endregion

    private static void CopyStringContent(string dest, int destPos, string src)
    {
        if (src.Length > dest.Length - destPos)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        SpanHelpers.Memmove(
            ref Unsafe.Add(ref dest._firstChar, destPos),
            ref src._firstChar,
            (uint)src.Length);

        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref dest._firstChar, destPos + src.Length) = '\0';
    }

    private string InternalSubString(int startIndex, int length)
    {
        if (startIndex < 0 || startIndex > Length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        if (length < 0 || startIndex > Length - length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        string result = FastNewString(length);
        SpanHelpers.Memmove(
            ref result._firstChar,
            //force zero-extension
            ref Unsafe.Add(ref _firstChar, (nint)(uint)startIndex),
            (uint)length);
        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref result._firstChar, length) = '\0';

        return result;
    }

    private static bool EqualsHelper(string a, string b)
    {
        if (a.Length != b.Length)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }


    private static unsafe string FastNewString(int numChars)
    {
        //always add a char for null termination
        string str = NewString("".m_pEEType, numChars + 1);
        //this indicates that the string is dynamically allocated and can be disposed of
        str._stringLength |= 1 << 31;
        str._stringLength--;
        return str;

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpNewArray")]
        static extern string NewString(MethodTable* pMt, int numElements);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CopyCharsNullTerminating(string dest, char* src, int length)
    {
        SpanHelpers.Memmove(ref dest._firstChar, ref *src, (nuint)length);
        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref dest._firstChar, length) = '\0';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyCharsNullTerminating(string dest, char[] src)
    {
        for (int i = 0; i < src.Length; i++)
        {
            Unsafe.Add(ref dest._firstChar, i) = src[i];
        }

        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref dest._firstChar, src.Length) = '\0';
    }
}