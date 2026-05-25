using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace ChihuahuaOS.CoreLib.Extra;

[StructLayout(LayoutKind.Sequential)]
public struct StaticRef<T>
{
    private nint _value;

    public StaticRef()
    {
    }

    public StaticRef(T value)
    {
        SetValue(value);
    }

    public T? GetValue()
    {
        if (_value == 0)
        {
            return default;
        }

        return Unsafe.As<nint, T>(ref _value);
    }

    public bool HasValue()
    {
        return _value != 0;
    }

    public void SetValue(T value)
    {
        Unsafe.As<nint, T>(ref _value) = value;
    }
}