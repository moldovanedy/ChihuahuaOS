using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChihuahuaOS.CoreLib.Extra.Runtime;

namespace System;

public abstract class Array : IDisposable
{
    private static class EmptyArray<T>
    {
        internal static readonly T[] Value = [];
    }

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

    public void Dispose()
    {
        MemUtils.FreeMemory(this);
    }
}

internal class Array<T> : Array
{
}

[StructLayout(LayoutKind.Sequential)]
internal class RawArrayData
{
    public uint Length;

    // Array._numComponents padded to IntPtr
    public uint Padding;

    public byte Data;
}