using System.Runtime;
using System.Runtime.CompilerServices;
using Extra.Runtime;
using Internal.Runtime.CompilerHelpers;

namespace System;

public sealed class String : IDisposable
{
    //internal ordering! DO NOT modify; also that's how it's supposed to have the auto-property
#pragma warning disable CS0649 // Field is never assigned to
    //intrinsic, no need to assign
    private readonly int _stringLength;
#pragma warning restore CS0649 // Field  never assigned to

    private char _firstChar;

    #region Properties

    // ReSharper disable once ConvertToAutoProperty
    public int Length => _stringLength;

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

    public override string ToString()
    {
        return this;
    }

    #endregion


    #region Static methods

    public static string Concat(string? str0, string? str1)
    {
        str0 ??= Empty;
        str1 ??= Empty;

        if (IsNullOrEmpty(str0))
        {
            return IsNullOrEmpty(str1) ? Empty : str1;
        }

        if (IsNullOrEmpty(str1))
        {
            return str0;
        }

        int totalLength = str0.Length + str1.Length;

        //can't overflow to a positive number, so just check < 0
        if (totalLength < 0)
        {
            ThrowHelpers.ThrowOverflowException();
        }

        string result = FastNewString(totalLength);
        CopyStringContent(result, 0, str0);
        CopyStringContent(result, str0.Length, str1);

        return result;
    }

    public static string Concat(string? str0, string? str1, string? str2)
    {
        str0 ??= Empty;
        str1 ??= Empty;
        str2 ??= Empty;

        if (IsNullOrEmpty(str0))
        {
            return Concat(str1, str2);
        }

        if (IsNullOrEmpty(str1))
        {
            return Concat(str0, str2);
        }

        if (IsNullOrEmpty(str2))
        {
            return Concat(str0, str1);
        }

        //it can overflow to a positive number, so we accumulate the total length as a long.
        long totalLength = (long)str0.Length + str1.Length + str2.Length;

        if (totalLength > int.MaxValue)
        {
            ThrowHelpers.ThrowOverflowException();
        }

        string result = FastNewString((int)totalLength);
        CopyStringContent(result, 0, str0);
        CopyStringContent(result, str0.Length, str1);
        CopyStringContent(result, str0.Length + str1.Length, str2);

        return result;
    }

    public static string Concat(string? str0, string? str1, string? str2, string? str3)
    {
        str0 ??= Empty;
        str1 ??= Empty;
        str2 ??= Empty;
        str3 ??= Empty;

        if (IsNullOrEmpty(str0))
        {
            return Concat(str1, str2, str3);
        }

        if (IsNullOrEmpty(str1))
        {
            return Concat(str0, str2, str3);
        }

        if (IsNullOrEmpty(str2))
        {
            return Concat(str0, str1, str3);
        }

        if (IsNullOrEmpty(str3))
        {
            return Concat(str0, str1, str2);
        }

        //it can overflow to a positive number, so we accumulate the total length as a long.
        long totalLength = (long)str0.Length + str1.Length + str2.Length + str3.Length;

        if (totalLength > int.MaxValue)
        {
            ThrowHelpers.ThrowOverflowException();
        }

        string result = FastNewString((int)totalLength);
        CopyStringContent(result, 0, str0);
        CopyStringContent(result, str0.Length, str1);
        CopyStringContent(result, str0.Length + str1.Length, str2);
        CopyStringContent(result, str0.Length + str1.Length + str2.Length, str3);

        return result;
    }

    [Intrinsic] // Unrolled and vectorized for half-constant input
    public static bool Equals(string? a, string? b)
    {
        if (a is null || b is null || a.Length != b.Length)
        {
            return false;
        }

        return EqualsHelper(a, b);
    }

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
        MemUtils.FreeMemory(this);
    }

    #endregion

    private static void CopyStringContent(string dest, int destPos, string src)
    {
        if (src.Length <= dest.Length - destPos)
        {
            ThrowHelpers.ThrowArgumentException();
        }

        SpanHelpers.Memmove(
            ref Unsafe.Add(ref dest._firstChar, destPos),
            ref src._firstChar,
            (uint)src.Length);

        //this is safe, as we always allocate one more char in string
        Unsafe.Add(ref dest._firstChar, src.Length) = '\0';
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
        return NewString("".m_pMethodTable, numChars + 1);

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