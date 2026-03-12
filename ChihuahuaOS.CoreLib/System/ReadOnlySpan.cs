using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Internal.Runtime.CompilerHelpers;

namespace System;

[Intrinsic]
public readonly ref struct ReadOnlySpan<T>
{
    private readonly ref T _reference;
    private readonly int _length;

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public int Length
    {
        [Intrinsic] get => _length;
    }

    public bool IsEmpty => _length == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ReadOnlySpan(T[]? array)
    {
        if (array == null)
        {
            this = default;
            return; //returns default
        }

        _reference = ref MemoryMarshal.GetArrayDataReference(array);
        _length = array.Length;
    }

    public unsafe ReadOnlySpan(void* pointer, int length)
    {
        _reference = ref Unsafe.As<byte, T>(ref *(byte*)pointer);
        _length = length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan(T[]? array, int start, int length)
    {
        if (array == null)
        {
            if (start != 0 || length != 0)
            {
                ThrowHelpers.ThrowArgumentException();
            }

            this = default;
            return; // returns default
        }
#if X64 || ARM64
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
                Environment.FailFast(null);
#elif X86 || ARM
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                Environment.FailFast(null);
#endif

        _reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), (nint)(uint)start);
        _length = length;
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ref readonly T this[int index]
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

    public static implicit operator ReadOnlySpan<T>(T[] array)
    {
        return new ReadOnlySpan<T>(array);
    }
}