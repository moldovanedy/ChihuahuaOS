namespace System.Runtime;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class RuntimeExportAttribute : Attribute
{
    public string EntryPoint;

    public RuntimeExportAttribute(string entry)
    {
        EntryPoint = entry;
    }
}