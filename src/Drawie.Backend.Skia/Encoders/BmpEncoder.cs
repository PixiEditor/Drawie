using BmpSharp;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Bitmap = BmpSharp.Bitmap;

namespace Drawie.Skia.Encoders;

public class BmpEncoder : IImageEncoder
{
    public byte[] Encode(Image image)
    {
        Image toEncode;
        using DrawingSurface surface = DrawingSurface.Create(new ImageInfo(
            image.Info.Width,
            image.Info.Height,
            ColorType.Bgra8888,
            AlphaType.Premul,
            image.Info.ColorSpace)
        );

        surface.Canvas.DrawImage(image, 0, 0);
        toEncode = surface.Snapshot();

        var bitsPerPixel = toEncode.Info.BytesPerPixel == 4 ? BitsPerPixelEnum.RGBA32 : BitsPerPixelEnum.RGB24;

        using var pixmap = toEncode.PeekPixels();
        byte[] imgBytes = pixmap.GetPixelSpan<byte>().ToArray();

        BmpSharp.Bitmap bitmap = new Bitmap(toEncode.Width, toEncode.Height, imgBytes, bitsPerPixel);

        if (toEncode != image)
            toEncode.Dispose();

        return bitmap.GetBmpBytes(true);
    }
}
