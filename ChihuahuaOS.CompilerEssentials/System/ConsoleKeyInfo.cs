namespace System;

public readonly struct ConsoleKeyInfo
{
    public char KeyChar { get; }
    public ConsoleKey Key { get; }
    public ConsoleModifiers Modifiers { get; }

    public ConsoleKeyInfo(char keyChar, ConsoleKey consoleKey, bool shift, bool alt, bool control)
    {
        KeyChar = keyChar;
        Key = consoleKey;

        if (shift)
        {
            Modifiers |= ConsoleModifiers.Shift;
        }

        if (alt)
        {
            Modifiers |= ConsoleModifiers.Alt;
        }

        if (control)
        {
            Modifiers |= ConsoleModifiers.Control;
        }
    }
}