namespace ChihuahuaOS.Kernel.FramebufferManager;

public readonly struct SolidColor
{
    public readonly byte Red;
    public readonly byte Green;
    public readonly byte Blue;

    public SolidColor()
    {
        Red = 0;
        Green = 0;
        Blue = 0;
    }

    public SolidColor(byte r, byte g, byte b)
    {
        Red = r;
        Green = g;
        Blue = b;
    }

    public SolidColor(uint color)
    {
        Red = (byte)(color >> 16);
        Green = (byte)(color >> 8);
        Blue = (byte)color;
    }

    public uint ToUint()
    {
        return (uint)((Red << 16) | (Green << 8) | Blue);
    }
}