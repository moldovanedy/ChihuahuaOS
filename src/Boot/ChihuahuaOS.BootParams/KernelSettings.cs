using System.Collections.Generic;
using ChihuahuaOS.MinimalUtils.Toml;

namespace ChihuahuaOS.BootParams;

public struct KernelSettings
{
    public const int NUM_SETTINGS = 2;

    public const string DISPLAY_HASH_NAME = "Display";

    #region Display

    public uint ScreenWidth = 0;
    public uint ScreenHeight = 0;

    #endregion

    public KernelSettings()
    {
    }

    public static KernelSettings FromConfigList(List<TomlSetting> settings)
    {
        KernelSettings kSettings = new();
        foreach (TomlSetting setting in settings)
        {
            switch (setting.Key)
            {
                case nameof(ScreenWidth):
                {
                    if (uint.TryParse(setting.Value, out uint screenWidth))
                    {
                        kSettings.ScreenWidth = screenWidth;
                    }

                    break;
                }
                case nameof(ScreenHeight):
                {
                    if (uint.TryParse(setting.Value, out uint screenHeight))
                    {
                        kSettings.ScreenHeight = screenHeight;
                    }

                    break;
                }
            }
        }

        return kSettings;
    }

    public List<TomlSetting> ToConfigList()
    {
        List<TomlSetting> configList = new(NUM_SETTINGS);

        configList.Add(
            new TomlSetting(nameof(ScreenWidth), TomlType.Integer, ScreenWidth.ToString(), DISPLAY_HASH_NAME));
        configList.Add(
            new TomlSetting(nameof(ScreenHeight), TomlType.Integer, ScreenHeight.ToString(), DISPLAY_HASH_NAME));

        return configList;
    }
}