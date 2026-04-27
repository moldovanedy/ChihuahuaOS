namespace System.Runtime.Versioning;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class TargetFrameworkAttribute : Attribute
{
    // The target framework moniker that this assembly was compiled against.
    // Use the FrameworkName class to interpret target framework monikers.
    public string FrameworkName { get; }

    public string? FrameworkDisplayName { get; set; }

    // The frameworkName parameter is intended to be the string form of a FrameworkName instance.
    public TargetFrameworkAttribute(string frameworkName)
    {
        FrameworkName = frameworkName;
    }
}