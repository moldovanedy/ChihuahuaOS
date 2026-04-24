using System.Runtime.CompilerServices;
using ChihuahuaOS.CoreLib.Extra.Runtime;
using Internal.Runtime.CompilerHelpers;

namespace System.Collections.Generic;

public class List<T> : IList<T>, IReadOnlyList<T>, IDisposable
{
    private const int DEFAULT_CAPACITY = 4;

    private T[] _items; // Do not rename (binary serialization)
    private int _size; // Do not rename (binary serialization)

    // ReSharper disable once NotAccessedField.Local
    private int _version; // Do not rename (binary serialization)

    private static readonly T[] EmptyArray = [];

    public int Capacity
    {
        get => _items.Length;
        set
        {
            if (value < _size)
            {
                ThrowHelpers.ThrowArgumentException();
                return;
            }

            if (value == _items.Length)
            {
                return;
            }

            if (value <= 0)
            {
                _items = EmptyArray;
            }

            T[] newItems = new T[value];
            if (_size > 0)
            {
                Array.Copy(_items, newItems, _size);
                _items.Dispose();
            }

            _items = newItems;
        }
    }

    public int Count => _size;

    public bool IsReadOnly => false;

    //the DefaultCapacity will be set at the first Add call
    public List()
    {
        _items = EmptyArray;
    }

    public List(int capacity)
    {
        switch (capacity)
        {
            case < 0:
                ThrowHelpers.ThrowArgumentOutOfRangeException();
                _items = EmptyArray;
                break;
            case 0:
                _items = EmptyArray;
                break;
            default:
                _items = new T[capacity];
                break;
        }

        _size = 0;
    }

    public List(IEnumerable<T> collection)
    {
        if (collection is ICollection<T> col)
        {
            int count = col.Count;
            if (count == 0)
            {
                _items = EmptyArray;
            }
            else
            {
                _items = new T[count];
                col.CopyTo(_items, 0);
                _size = count;
            }
        }
        else
        {
            _items = EmptyArray;
            using IEnumerator<T> en = collection.GetEnumerator();
            while (en.MoveNext())
            {
                Add(en.Current);
            }
        }
    }

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_size)
            {
                ThrowHelpers.ThrowArgumentOutOfRangeException();
            }

            return _items[index];
        }
        set
        {
            if ((uint)index >= (uint)_size)
            {
                ThrowHelpers.ThrowArgumentOutOfRangeException();
            }

            _items[index] = value;
            _version++;
        }
    }

    public void Dispose()
    {
        _items.Dispose();
        MemUtils.FreeMemory(this);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new ArrayEnumerator<T>(_items);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        _version++;
        int size = _size;
        if ((uint)size >= (uint)_items.Length)
        {
            Grow(size + 1);
        }

        _size = size + 1;
        _items[size] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _version++;
        int size = _size;
        _size = 0;

        if (size > 0)
        {
            Array.Clear(_items, 0, size);
        }
    }

    public bool Contains(T value)
    {
        return IndexOf(value) != -1;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    public void CopyTo(T[] array)
    {
        Array.Copy(_items, 0, array, 0, _size);
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, _size);
    }

    public int IndexOf(T item, int startIndex)
    {
        return Array.IndexOf(_items, item, startIndex, _size - startIndex);
    }

    public int IndexOf(T item, int startIndex, int count)
    {
        return Array.IndexOf(_items, item, startIndex, count);
    }

    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_size)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        if (_size == _items.Length)
        {
            GrowForInsertion(index, 1);
        }
        else if (index < _size)
        {
            Array.Copy(_items, index, _items, index + 1, _size - index);
        }

        _items[index] = item;
        _size++;
        _version++;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_size)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException();
        }

        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }

        _items[_size] = default!;
        _version++;
    }


    private void Grow(int newSize)
    {
        Capacity = GetNewCapacity(newSize);
    }

    private void GrowForInsertion(int indexToInsert, int insertionCount = 1)
    {
        int requiredCapacity = checked(_size + insertionCount);
        int newCapacity = GetNewCapacity(requiredCapacity);

        T[] newItems = new T[newCapacity];
        if (indexToInsert != 0)
        {
            Array.Copy(_items, newItems, indexToInsert);
        }

        if (_size != indexToInsert)
        {
            Array.Copy(_items, indexToInsert, newItems, indexToInsert + insertionCount, _size - indexToInsert);
        }

        _items = newItems;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewCapacity(int capacity)
    {
        int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : 2 * _items.Length;

        // Allow the list to grow to maximum possible capacity (int.MaxValue elements) before encountering overflow.
        if ((uint)newCapacity > Array.MaxLength)
        {
            newCapacity = Array.MaxLength;
        }

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newCapacity < capacity)
        {
            newCapacity = capacity;
        }

        return newCapacity;
    }
}