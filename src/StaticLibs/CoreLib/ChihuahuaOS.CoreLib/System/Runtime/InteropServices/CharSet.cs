namespace System.Runtime.InteropServices;

public enum CharSet
{
    /// <summary>
    /// Did not specify how to marshal strings
    /// </summary>
    None = 1,

    /// <summary>
    /// Strings should be marshaled as ANSI 1 byte chars.
    /// </summary>
    Ansi = 2,

    /// <summary>
    /// Strings should be marshaled as Unicode 2 byte chars (UTF-16).
    /// </summary>
    Unicode = 3,

    /// <summary>
    /// Marshal strings in the right way for the target system.
    /// </summary>
    Auto = 4
}