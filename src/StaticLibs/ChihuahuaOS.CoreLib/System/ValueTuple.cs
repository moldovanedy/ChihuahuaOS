using Internal.Runtime.CompilerHelpers;

namespace System;

public readonly struct ValueTuple<T1>
{
    public readonly T1 Item1;

    /// <summary>
    /// The number of positions in this data structure.
    /// </summary>
    public int Length => 2;

    public object? this[int index]
    {
        get
        {
            if (index != 0)
            {
                ThrowHelpers.ThrowIndexOutOfRangeException();
                return null;
            }

            return Item1;
        }
    }

    public ValueTuple(T1 t1)
    {
        Item1 = t1;
    }
}

public readonly struct ValueTuple<T1, T2>
{
    public readonly T1 Item1;
    public readonly T2 Item2;

    /// <summary>
    /// The number of positions in this data structure.
    /// </summary>
    public int Length => 2;

    public object? this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return Item1;
                case 1:
                    return Item2;
                default:
                    ThrowHelpers.ThrowIndexOutOfRangeException();
                    return null;
            }
        }
    }

    public ValueTuple(T1 t1, T2 t2)
    {
        Item1 = t1;
        Item2 = t2;
    }
}

public readonly struct ValueTuple<T1, T2, T3>
{
    public readonly T1 Item1;
    public readonly T2 Item2;
    public readonly T3 Item3;

    /// <summary>
    /// The number of positions in this data structure.
    /// </summary>
    public int Length => 3;

    public object? this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return Item1;
                case 1:
                    return Item2;
                case 2:
                    return Item3;
                default:
                    ThrowHelpers.ThrowIndexOutOfRangeException();
                    return null;
            }
        }
    }

    public ValueTuple(T1 t1, T2 t2, T3 t3)
    {
        Item1 = t1;
        Item2 = t2;
        Item3 = t3;
    }
}