using System.Runtime;

namespace System;

public unsafe class Object
{
    internal MethodTable* MethodTable => m_pEEType;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    // The layout of object is a contract with the compiler.
    // ReSharper disable once InconsistentNaming
    internal MethodTable* m_pEEType;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    // Allow an object to free resources before the GC reclaims the object.
    // This method's virtual slot number is hardcoded in runtimes. Do not add any virtual methods ahead of this.
#pragma warning disable CA1821 // Remove empty Finalizers
    // ReSharper disable once EmptyDestructor
    ~Object()
    {
    }
#pragma warning restore CA1821

    /// <summary>
    /// Returns a string that represents the current object. It will always allocate heap memory for it, so you
    /// can safely free it after you no longer need it.
    /// </summary>
    public virtual string ToString()
    {
        // The default for an object is to return the fully qualified name of the class.
        return "";
    }
}