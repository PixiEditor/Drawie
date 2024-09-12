using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public sealed class SkiaColorImplementation : IColorImplementation
    {
        public ColorF ColorToColorF(uint colorValue)
        {
            SKColor color = new SKColor(colorValue);
            return SKColorFToColorF((SKColorF)color);
        }

        public Color ColorFToColor(ColorF color)
        {
            SKColorF skColorF = ColorFToSKColorF(color);
            SKColor skColor = (SKColor)skColorF;
            return new Color(skColor.Red, skColor.Green, skColor.Blue, skColor.Alpha);
        }

        public ColorType GetPlatformColorType()
        {
            SKColorType colorType = SKImageInfo.PlatformColorType;
            return (ColorType)colorType;
        }
        
        public static ColorF SKColorFToColorF(SKColorF color)
        {
            return new ColorF(color.Red, color.Green, color.Blue, color.Alpha);
        }
        
        public static SKColorF ColorFToSKColorF(ColorF color)
        {
            return new SKColorF(color.R, color.G, color.B, color.A);
        }
    }
}
