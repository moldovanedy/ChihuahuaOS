namespace System;

[AttributeUsage(AttributeTargets.Class)]
public class AttributeUsageAttribute : Attribute
{
    public AttributeTargets ValidOn { get; }
    public bool AllowMultiple { get; set; }
    public bool Inherited { get; set; }

    internal static readonly AttributeUsageAttribute Default = new(AttributeTargets.All);

    public AttributeUsageAttribute(AttributeTargets validOn)
    {
        ValidOn = validOn;
        Inherited = true;
    }

    internal AttributeUsageAttribute(AttributeTargets validOn, bool allowMultiple, bool inherited)
    {
        ValidOn = validOn;
        AllowMultiple = allowMultiple;
        Inherited = inherited;
    }
}