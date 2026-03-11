namespace System;

/// <summary>
/// Specifies that an object has heap memory and needs to be manually freed. Use the "using" statement as much as
/// possible to avoid memory leaks.
/// </summary>
public interface IDisposable
{
    /// <summary>
    /// Frees the object memory.
    /// </summary>
    public void Dispose();
}