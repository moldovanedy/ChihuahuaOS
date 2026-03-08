namespace System.Runtime;

//this is a contract with the compiler, don't change the ordering and names
public unsafe struct MethodTable
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    // ReSharper disable InconsistentNaming

    internal ushort _usComponentSize;
    private ushort _usFlags;
    internal uint _uBaseSize;
    internal MethodTable* _relatedType;
    private ushort _usNumVtableSlots;
    private ushort _usNumInterfaces;
    private uint _uHashCode;

    // ReSharper restore InconsistentNaming
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}