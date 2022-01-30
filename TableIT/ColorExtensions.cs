#nullable enable
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TableIT;

public static class ColorExtensions
{
    public static uint ToPackedColor(this Brush brush)
    {
        if (brush is SolidColorBrush solidBrush)
        {
            uint packedColor =(uint)(
                (solidBrush.Color.A << 24) |
                (solidBrush.Color.R << 16) |
                (solidBrush.Color.G << 8) |
                (solidBrush.Color.B << 0)
                );
            return packedColor;
        }
        return 0x0;
    }

    public static Brush ToBrush(this uint packedColor)
    {
        Color color = Color.FromArgb(
            (byte)(packedColor >> 24 & 0xFF),
            (byte)(packedColor >> 16 & 0xFF),
            (byte)(packedColor >> 8 & 0xFF),
            (byte)(packedColor >> 0 & 0xFF));
        return new SolidColorBrush(color);
    }
}
