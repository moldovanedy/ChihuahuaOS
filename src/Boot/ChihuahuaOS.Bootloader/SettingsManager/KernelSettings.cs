using System.Collections.Generic;

namespace ChihuahuaOS.Bootloader.SettingsManager;

//TODO: move this to a shared project

public struct KernelSettings
{
    public const int NUM_SETTINGS = 2;

    public const string DISPLAY_HASH_NAME = "Display";

    #region Display

    public int ScreenWidth = 1920;
    public int ScreenHeight = 1080;

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
                    if (int.TryParse(setting.Value, out int screenWidth))
                    {
                        kSettings.ScreenWidth = screenWidth;
                    }

                    break;
                }
                case nameof(ScreenHeight):
                {
                    if (int.TryParse(setting.Value, out int screenHeight))
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