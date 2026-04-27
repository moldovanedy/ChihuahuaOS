using ChihuahuaOS.CoreLib;

namespace System.Reflection;

public abstract class MemberInfo : ICustomAttributeProvider
{
    public abstract MemberTypes MemberType { get; }
    public abstract string Name { get; }
    public abstract Type? DeclaringType { get; }
    public abstract Type? ReflectedType { get; }

    // public virtual Module Module
    // {
    //     get
    //     {
    //         // This check is necessary because for some reason, Type adds a new "Module" property that hides the inherited one instead
    //         // of overriding.
    //
    //         if (this is Type type)
    //             return type.Module;
    //
    //         throw NotImplemented.ByDesign;
    //     }
    // }

    public virtual unsafe bool HasSameMetadataDefinitionAs(MemberInfo other)
    {
        CoreLibManager.Panic("MemberInfo.HasSameMetadataDefinitionAs called".ToCharPtrUnsafe());
        return false;
    }

    public abstract object[] GetCustomAttributes(bool inherit);

    public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

    public abstract bool IsDefined(Type attributeType, bool inherit);

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static bool operator ==(MemberInfo? left, MemberInfo? right)
    // {
    //     // Test "right" first to allow branch elimination when inlined for null checks (== null)
    //     // so it can become a simple test
    //     if (right is null)
    //     {
    //         return left is null;
    //     }
    //
    //     // Try fast reference equality and opposite null check prior to calling the slower virtual Equals
    //     if (ReferenceEquals(left, right))
    //     {
    //         return true;
    //     }
    //
    //     return left is not null && left.Equals(right);
    // }
    //
    // public static bool operator !=(MemberInfo? left, MemberInfo? right) => !(left == right);
}