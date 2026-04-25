using System;
using ChihuahuaOS.CoreLib.Extra.Runtime;

namespace ChihuahuaOS.CoreLib.Extra;

public partial class SharedPtr<T> : IDisposable where T : IDisposable
{
    public bool IsValueDisposed { get; private set; }

    private T _value;
    private int _numReferences;

    public SharedPtr(T value)
    {
        _value = value;
    }

    public Window Get()
    {
        return new Window(this);
    }

    /// <summary>
    /// NOTE: this only works if the value is already disposed (<see cref="IsValueDisposed"/>), otherwise will do
    /// nothing.
    /// </summary>
    public void Dispose()
    {
        if (IsValueDisposed)
        {
            MemUtils.FreeMemory(this);
        }
    }
}