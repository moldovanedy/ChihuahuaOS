namespace System;

public struct Nullable<T> where T : struct
{
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public readonly bool HasValue => _hasValue;

    private readonly bool _hasValue; // Do not rename (binary serialization)

    public readonly T Value
    {
        get
        {
            if (!_hasValue)
                Environment.FailFast("Null reference");
            return _value;
        }
    }

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private T _value; // Do not rename (binary serialization)

    public Nullable(T value)
    {
        (_hasValue, _value) = (true, value);
    }

    public static implicit operator T?(T value)
    {
        return value;
    }

    public static explicit operator T(T? value)
    {
        return value!.Value;
    }
}