namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false)]
public sealed class MethodImplAttribute : Attribute
{
    // public MethodCodeType MethodCodeType;
    public MethodImplOptions Value { get; }

    public MethodImplAttribute(MethodImplOptions methodImplOptions)
    {
        Value = methodImplOptions;
    }

    public MethodImplAttribute(short value)
    {
        Value = (MethodImplOptions)value;
    }

    public MethodImplAttribute()
    {
    }
}