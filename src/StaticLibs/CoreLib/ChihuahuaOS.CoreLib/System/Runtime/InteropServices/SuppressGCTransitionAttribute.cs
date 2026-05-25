namespace System.Runtime.InteropServices;

/// <summary>
/// An attribute used to indicate a GC transition should be skipped when making an unmanaged function call.
/// </summary>
/// <example>
/// Example of a valid use case. The Win32 `GetTickCount()` function is a small performance related function
/// that reads some global memory and returns the value. In this case, the GC transition overhead is significantly
/// more than the memory read.
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
// ReSharper disable once InconsistentNaming
public sealed class SuppressGCTransitionAttribute : Attribute
{
}