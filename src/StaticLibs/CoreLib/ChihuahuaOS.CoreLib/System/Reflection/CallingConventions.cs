namespace System.Reflection;

[Flags]
public enum CallingConventions
{
    // Keep in sync with COMMember.cpp.
    Standard = 0x0001,
    VarArgs = 0x0002,
    Any = Standard | VarArgs,
    HasThis = 0x0020,
    ExplicitThis = 0x0040
}