namespace System.Collections;

public interface IEnumerator
{
    /// <summary>
    /// Advances the enumerator in the enumeration if possible (otherwise it will return false).
    /// </summary>
    /// <returns>True if the enumerator advanced, false otherwise (i.e., it reached the end).</returns>
    bool MoveNext();

#nullable disable // to avoid warnings in foreach
    /// <summary>
    /// Returns the element at which the enumerator currently points to.
    /// </summary>
    object Current { get; }

    /// <summary>
    /// Resets the enumerator to the beginning of the enumeration.
    /// </summary>
    void Reset();
}