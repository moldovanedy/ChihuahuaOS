namespace System;

/// <summary>
/// Indicates that a method will allow a variable number of arguments in its invocation.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
public sealed class ParamArrayAttribute : Attribute
{
}