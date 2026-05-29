using System.Runtime;

namespace Internal;

/// <summary>
/// Contains methods requested by the linker, but with no definition found in the .NET code or some with an unclear
/// assembly implementation.
/// </summary>
public static class UnknownExports
{
    [RuntimeExport("RhpStackProbe")]
    private static void RhpStackProbe()
    {
    }
}
