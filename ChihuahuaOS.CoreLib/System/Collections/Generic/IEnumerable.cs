using System.Runtime.CompilerServices;

namespace System.Collections.Generic;

public interface IEnumerable<out T> : IEnumerable where T : allows ref struct
{
    [Intrinsic]
    new IEnumerator<T> GetEnumerator();
}