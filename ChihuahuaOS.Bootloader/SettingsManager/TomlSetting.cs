using System;
using ChihuahuaOS.CoreLib.Extra.Runtime;

namespace ChihuahuaOS.Bootloader.SettingsManager;

public class TomlSetting : IDisposable
{
    public string Key { get; }
    public TomlType DataType { get; }
    public string Value { get; }
    public string Hash { get; }

    public TomlSetting(string key, TomlType type, string value, string hash = "")
    {
        Key = key;
        DataType = type;
        Value = value;
        Hash = hash;
    }

    public void Dispose()
    {
        MemUtils.FreeMemory(this);
    }
}