using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public class AvaloniaBitmapTexture : ITexture
{
    public WriteableBitmap Bitmap { get; }

    public AvaloniaBitmapTexture(PixelSize size)
    {
        Bitmap = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
    }
}

