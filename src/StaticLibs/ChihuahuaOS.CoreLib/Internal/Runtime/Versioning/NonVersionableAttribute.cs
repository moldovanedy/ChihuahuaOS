namespace System.Runtime.Versioning;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor,
    Inherited = false)]
internal sealed class NonVersionableAttribute : Attribute
{
    public NonVersionableAttribute()
    {
    }
}