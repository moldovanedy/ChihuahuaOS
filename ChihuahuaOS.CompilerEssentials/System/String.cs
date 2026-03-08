using System.Runtime;
using System.Runtime.CompilerServices;

namespace System;

public sealed class String
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    // The layout of the string type is a contract with the compiler.
    private readonly int _length;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    private char _firstChar;

    public int Length => _length;

    [IndexerName("Chars")]
    public char this[int index]
    {
        [Intrinsic] get => Unsafe.Add(ref _firstChar, index);
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(char* value, int length);

    private static unsafe string Ctor(char* ptr)
    {
        char* cursor = ptr;
        while (*cursor++ != 0) ;

        string result = FastNewString((int)(cursor - ptr - 1));
        for (int i = 0; i < cursor - ptr - 1; i++)
        {
            Unsafe.Add(ref result._firstChar, i) = ptr[i];
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern unsafe String(sbyte* value, int length);

    private static unsafe string Ctor(sbyte* ptr)
    {
        sbyte* cursor = ptr;
        while (*cursor++ != 0) ;

        string result = FastNewString((int)(cursor - ptr - 1));
        for (int i = 0; i < cursor - ptr - 1; i++)
        {
            Unsafe.Add(ref result._firstChar, i) = (char)ptr[i];
        }

        return result;
    }

    private static unsafe string FastNewString(int numChars)
    {
        return NewString("".m_pMethodTable, numChars);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [RuntimeImport("*", "RhpNewArray")]
        static extern string NewString(MethodTable* pMt, int numElements);
    }
}