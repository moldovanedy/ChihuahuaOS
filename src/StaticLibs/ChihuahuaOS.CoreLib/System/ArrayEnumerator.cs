using System.Collections;
using System.Collections.Generic;

namespace System;

public class ArrayEnumerator<T> : IEnumerator<T>
{
    private readonly T[] _array;
    private int _index;

    internal ArrayEnumerator(T[] array)
    {
        _array = array;
        _index = -1;
    }

    public bool MoveNext()
    {
        _index++;
        if (_index >= _array.Length)
        {
            _index = _array.Length;
            return false;
        }

        return true;
    }

    T IEnumerator<T>.Current
    {
        get
        {
            if (_index >= _array.Length || _index < 0)
            {
                return default!;
            }

            return _array[_index];
        }
    }

    object? IEnumerator.Current
    {
        get
        {
            if (_index >= _array.Length || _index < 0)
            {
                return null;
            }

            return _array[_index];
        }
    }

    public void Reset()
    {
        _index = -1;
    }

    public void Dispose()
    {
    }
}