namespace System.Collections;

public interface ICollection : IEnumerable
{
    /// <summary>
    /// Copies the collection data to a given array.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="index">The index in the destination array at which to start the copying.</param>
    void CopyTo(Array array, int index);

    /// <summary>
    /// The number of items in the collection.
    /// </summary>
    int Count { get; }

    //TODO
    // object SyncRoot { get; }

    /// <summary>
    /// Is the collection thread-safe?
    /// </summary>
    bool IsSynchronized { get; }
}