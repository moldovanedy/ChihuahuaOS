using Internal.Runtime.CompilerHelpers;

namespace System;

public abstract class Delegate
{
    // ReSharper disable NotAccessedField.Local
#pragma warning disable CS0169 // Field is never used

    private object? _firstParameter;
    private object? _helperObject;
    private nint _extraFunctionPointerOrData;
    private IntPtr _functionPointer;

#pragma warning restore CS0169 // Field is never used
    // ReSharper restore NotAccessedField.Local

    internal void InitializeClosedInstance(object? firstParameter, IntPtr functionPointer)
    {
        if (firstParameter is null)
        {
            ThrowHelpers.ThrowNullReferenceException();
            return;
        }

        _functionPointer = functionPointer;
        _firstParameter = firstParameter;
    }
}