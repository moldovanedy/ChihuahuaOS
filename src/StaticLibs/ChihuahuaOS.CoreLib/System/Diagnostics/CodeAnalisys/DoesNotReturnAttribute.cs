namespace System.Diagnostics.CodeAnalisys;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class DoesNotReturnAttribute : Attribute
{
}