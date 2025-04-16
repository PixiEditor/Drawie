using BmpSharp;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Bitmap = BmpSharp.Bitmap;

namespace Drawie.Skia.Encoders;

public class BmpEncoder : IImageEncoder
{
    public byte[] Encode(Image image)
    {
        Image toEncode = image;
        if (image.Info.BytesPerPixel is not (3 or 4))
        {
            using DrawingSurface surface = DrawingSurface.Create(new ImageInfo(
                image.Info.Width,
                image.Info.Height,
                ToValidColorType(image.Info.BytesPerPixel),
                AlphaType.Premul,
                image.Info.ColorSpace)
            );

            surface.Canvas.DrawImage(image, 0, 0);
            toEncode = surface.Snapshot();
        }

        var bitsPerPixel = toEncode.Info.BytesPerPixel == 4 ? BitsPerPixelEnum.RGBA32 : BitsPerPixelEnum.RGB24;

        using var pixmap = toEncode.PeekPixels();
        byte[] imgBytes = pixmap.GetPixelSpan<byte>().ToArray();

        BmpSharp.Bitmap bitmap = new Bitmap(toEncode.Width, toEncode.Height, imgBytes, bitsPerPixel);

        if (toEncode != image)
            toEncode.Dispose();

        return bitmap.GetBmpBytes(true);
    }

    private static ColorType ToValidColorType(int bytesPerPixel)
    {
        return bytesPerPixel switch
        {
            8 => ColorType.Bgra8888,
            _ => throw new ArgumentOutOfRangeException(nameof(bytesPerPixel),
                "Only 3 or 4 bytes per pixel are supported.")
        };
    }
}
