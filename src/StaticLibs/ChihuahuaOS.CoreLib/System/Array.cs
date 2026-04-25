using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib.Extra.Runtime;
using Internal.Runtime.CompilerHelpers;

namespace System;

public abstract class Array : IDisposable
{
    private static class EmptyArray<T>
    {
        internal static readonly T[] Value = [];
    }

    public const int MaxLength = int.MaxValue;

    // CS0169: The field 'Array._numComponents' is never used
#pragma warning disable 0169
    // This field should be the first field in Array as the runtime/compilers depend on it
    private readonly int _numComponents;
#pragma warning restore

    public int Length => (int)Unsafe.As<RawArrayData>(this).Length;


    // This ctor exists solely to prevent C# from generating a protected .ctor that violates the surface area.
    private protected Array()
    {
    }

    public static T[] Empty<T>()
    {
        return EmptyArray<T>.Value;
    }


    #region Static methods

    public static void Copy(Array sourceArray, Array destinationArray, long length)
    {
        int intLength = (int)length;
        if (intLength != length)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
            return;
        }

        Copy(sourceArray, destinationArray, intLength);
    }

    public static void Copy(Array sourceArray, Array destinationArray, int length)
    {
        if (sourceArray.Length != destinationArray.Length)
        {
            ThrowHelpers.ThrowArgumentException();
            return;
        }

        for (int i = 0; i < length; i++)
        {
            destinationArray[i] = sourceArray[i];
        }

        // fast copy
        // MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(sourceArray);
        // nuint byteCount = (uint)length * (nuint)pMethodTable->_usComponentSize;
        //
        // ref byte src = ref Unsafe.As<RawArrayData>(sourceArray).Data;
        // ref byte dst = ref Unsafe.As<RawArrayData>(destinationArray).Data;
        // SpanHelpers.Memmove(ref src, ref dst, byteCount);
    }

    public static void Copy(
        Array sourceArray,
        long sourceIndex,
        Array destinationArray,
        long destinationIndex,
        long length)
    {
        int intSourceIndex = (int)sourceIndex;
        int intDestinationIndex = (int)destinationIndex;
        int intLength = (int)length;

        if (sourceIndex != intSourceIndex)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        if (destinationIndex != intDestinationIndex)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        if (length != intLength)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        Copy(sourceArray, intSourceIndex, destinationArray, intDestinationIndex, intLength);
    }

    public static unsafe void Copy(
        Array sourceArray,
        int sourceIndex,
        Array destinationArray,
        int destinationIndex,
        int length)
    {
        if (sourceArray.Length != destinationArray.Length)
        {
            ThrowHelpers.ThrowArgumentException();
            return;
        }

        for (int i = 0; i < length; i++)
        {
            destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
        }

        //fast copy
        // MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(sourceArray);
        // nuint elementSize = pMethodTable->_usComponentSize;
        // nuint byteCount = (uint)length * elementSize;
        //
        // ref byte src = ref Unsafe.AddByteOffset(ref Unsafe.As<RawArrayData>(sourceArray).Data,
        //     (uint)sourceIndex * elementSize);
        // ref byte dst = ref Unsafe.AddByteOffset(ref Unsafe.As<RawArrayData>(destinationArray).Data,
        //     (uint)destinationIndex * elementSize);
        // SpanHelpers.Memmove(ref src, ref dst, byteCount);
    }

    public static unsafe void Clear(Array sourceArray)
    {
        MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(sourceArray);
        nuint elementSize = pMethodTable->_usComponentSize;
        nuint byteCount = (uint)sourceArray.Length * elementSize;

        SpanHelpers.Fill(ref Unsafe.As<RawArrayData>(sourceArray).Data, 0, byteCount);
    }

    public static unsafe void Clear(Array sourceArray, int index, int length)
    {
        MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(sourceArray);
        nuint elementSize = pMethodTable->_usComponentSize;
        nuint byteCount = (uint)length * elementSize;

        SpanHelpers.Fill(
            ref Unsafe.AddByteOffset(
                ref Unsafe.As<RawArrayData>(sourceArray).Data,
                (uint)index * elementSize),
            0,
            byteCount);
    }

    public static int IndexOf(Array array, object? value)
    {
        return IndexOf(array, value, 0, array.Length);
    }

    public static int IndexOf(Array array, object? value, int startIndex)
    {
        return IndexOf(array, value, startIndex, array.Length);
    }

    public static int IndexOf(Array array, object? value, int startIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (array[startIndex + i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion


    public unsafe object this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
            {
                ThrowHelpers.ThrowArgumentOutOfRangeException();
            }

            MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(this);
            nuint elementSize = pMethodTable->_usComponentSize;

            byte elemStart = Unsafe.AddByteOffset(
                ref Unsafe.As<RawArrayData>(this).Data,
                (uint)index * elementSize);
            nint address = (nint)(&elemStart);
            return Unsafe.As<nint, object>(ref address);
        }
        set
        {
            if (index < 0 || index >= Length)
            {
                ThrowHelpers.ThrowArgumentOutOfRangeException();
            }

            MethodTable* pMethodTable = RuntimeHelpers.GetMethodTable(this);
            nuint elementSize = pMethodTable->_usComponentSize;

            byte elemStart = Unsafe.AddByteOffset(
                ref Unsafe.As<RawArrayData>(this).Data,
                (uint)index * elementSize);
            nint address = (nint)(&elemStart);
            Unsafe.As<nint, object>(ref address) = value;
        }
    }

    public void Clear()
    {
        Clear(this);
    }

    public bool Contains(object? value)
    {
        return IndexOf(value) != -1;
    }

    public int IndexOf(object? value)
    {
        return IndexOf(this, value);
    }

    public void Dispose()
    {
        MemUtils.FreeMemory(this);
    }
}

internal class Array<T> : Array
{
}

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

// ReSharper disable once ClassNeverInstantiated.Global
[StructLayout(LayoutKind.Sequential)]
internal class RawArrayData
{
    public uint Length;

    // Array._numComponents padded to IntPtr
    public uint Padding;

    public byte Data;
}

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value