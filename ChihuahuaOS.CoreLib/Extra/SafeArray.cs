using System;

namespace Extra;

/// <summary>
/// A thin wrapper over a regular array, specially made for disposable types. It automatically disposes of all the
/// resources of the internal array when it itself is disposed. Note that it takes ownership of the given objects and
/// calls <see cref="IDisposable.Dispose"/> when disposed itself.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SafeArray<T> : IDisposable where T : IDisposable
{
    private readonly T[] _array;

    public int Length => _array.Length;

    public SafeArray(params T[] array)
    {
        _array = array;
    }

    public SafeArray(int size)
    {
        _array = new T[size];
    }

    public T this[int index]
    {
        get => _array[index];
        set => _array[index] = value;
    }

    public T[] GetInternalArray()
    {
        return _array;
    }

    public T[] ToArray()
    {
        return _array;
    }

    public void Dispose()
    {
        for (int i = 0; i < _array.Length; i++)
        {
            _array[i].Dispose();
        }

        _array.Dispose();
    }
}