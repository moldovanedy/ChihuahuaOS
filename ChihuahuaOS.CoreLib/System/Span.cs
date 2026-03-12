using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerHelpers;

namespace System;

public readonly ref struct Span<T>
{
    private readonly ref T _reference;
    private readonly int _length;

    public int Length => _length;
    public bool IsEmpty => _length == 0;

    public Span(T[]? array)
    {
        if (array == null)
        {
            this = default;
            _reference = default!;
            return;
        }

        _reference = ref MemoryMarshal.GetArrayDataReference(array);
        _length = array.Length;
    }

    public unsafe Span(void* pointer, int length)
    {
        _reference = ref Unsafe.As<byte, T>(ref *(byte*)pointer);
        _length = length;
    }

    public Span(T[]? array, int start, int length)
    {
        if (array == null)
        {
            if (start != 0 || length != 0)
            {
                ThrowHelpers.ThrowIndexOutOfRangeException();
                _reference = default!;
                return;
            }

            this = default;
            _reference = default!;
            return;
        }

        if ((uint)start + (uint)length > (uint)array.Length)
        {
            ThrowHelpers.ThrowIndexOutOfRangeException();
        }

// #if X64 || ARM64
//             if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
//                 Environment.FailFast(null);
// #elif X86 || ARM
//             if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
//                 Environment.FailFast(null);
// #else
//         Environment.FailFast("Unknown architecture");
// #endif

        _reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), (nint)(uint)start);
        _length = length;
    }

    public ref T this[int index]
    {
        [Intrinsic]
        get
        {
            if ((uint)index >= (uint)_length)
            {
                ThrowHelpers.ThrowIndexOutOfRangeException();
            }

            return ref Unsafe.Add(ref _reference, (nint)(uint)index);
        }
    }

    // /// <summary>
    // /// Returns false if left and right point at the same memory and have the same length.  Note that
    // /// this does *not* check to see if the *contents* are equal.
    // /// </summary>
    // public static bool operator !=(Span<T> left, Span<T> right)
    // {
    //     return !(left == right);
    // }
    //
    // /// <summary>
    // /// Returns true if left and right point at the same memory and have the same length.  Note that
    // /// this does *not* check to see if the *contents* are equal.
    // /// </summary>
    // public static bool operator ==(Span<T> left, Span<T> right)
    // {
    //     return left._length == right._length &&
    //            Unsafe.AreSame(ref left._reference, ref right._reference);
    // }

    public void Clear()
    {
        for (int i = 0; i < _length; i++)
        {
            Unsafe.Add(ref _reference, i) = default!;
        }
    }

    public void Fill(T value)
    {
        for (int i = 0; i < _length; i++)
        {
            Unsafe.Add(ref _reference, i) = value;
        }
    }
}