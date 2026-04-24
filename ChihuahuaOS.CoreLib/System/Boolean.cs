namespace System;

public struct Boolean
{
    // ReSharper disable ConvertToConstant.Global
    public static readonly string TrueString = "True";

    public static readonly string FalseString = "False";
    // ReSharper restore ConvertToConstant.Global

    #region Methods

    public override string ToString()
    {
        return this ? TrueString.Clone() : FalseString.Clone();
    }

    #endregion


    #region Static methods

    public static bool TryParse(string s, out bool result)
    {
        if (s.ToLowerInvariant() == "true")
        {
            result = true;
            return true;
        }

        if (s.ToLowerInvariant() == "false")
        {
            result = false;
            return true;
        }

        result = false;
        return false;
    }

    #endregion
}