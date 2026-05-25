using System;
using ChihuahuaOS.CoreLib.Extra.Runtime;

namespace ChihuahuaOS.CoreLib.Extra;

public partial class SharedPtr<T> : IDisposable where T : IDisposable
{
    public bool IsValueDisposed { get; private set; }

    private T _value;
    private int _numReferences;
    private readonly Window _selfReference;

    public SharedPtr(T value)
    {
        _value = value;
        _selfReference = Get();
    }

    public Window Get()
    {
        return new Window(this);
    }

    /// <summary>
    /// NOTE: this only works if there are no more active <see cref="Window"/> instances.
    /// </summary>
    public void Dispose()
    {
        _selfReference.Dispose();

        if (IsValueDisposed)
        {
            MemUtils.FreeMemory(this);
        }
    }
}