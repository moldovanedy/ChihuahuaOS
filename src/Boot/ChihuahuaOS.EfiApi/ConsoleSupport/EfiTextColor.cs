using System;

namespace ChihuahuaOS.EfiApi.ConsoleSupport;

[Flags]
public enum EfiTextColor : ulong
{
    Black = 0x00,
    Blue = 0x01,
    Green = 0x02,
    Cyan = 0x03,
    Red = 0x04,
    Magenta = 0x05,
    Brown = 0x06,
    LightGray = 0x07,
    DarkGray = 0x08,
    LightBlue = 0x09,
    LightGreen = 0x0A,
    LightCyan = 0x0B,
    LightRed = 0x0C,
    LightMagenta = 0x0D,
    Yellow = 0x0E,
    White = 0x0F,

    BackgroundBlack = Black,
    BackgroundBlue = 0x10,
    BackgroundGreen = 0x20,
    BackgroundCyan = 0x30,
    BackgroundRed = 0x40,
    BackgroundMagenta = 0x50,
    BackgroundBrown = 0x60,
    BackgroundLightGray = 0x70
}