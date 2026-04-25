using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

[Intrinsic]
public interface IEnumerator<out T> : IEnumerator, IDisposable
    where T : allows ref struct
{
    /// <inheritdoc cref="IEnumerator.Current"/> 
    new T Current { get; }
}