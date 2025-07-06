using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core;

public class Texture : IDisposable, ICloneable
{
    public VecI Size { get; }
    public DrawingSurface DrawingSurface { get; private set; }

    public event SurfaceChangedEventHandler? Changed;

    public bool IsDisposed => isDisposed || DrawingSurface.IsDisposed;
    public bool IsHardwareAccelerated { get; } = DrawingBackendApi.Current.IsHardwareAccelerated;

    public ColorSpace ColorSpace { get; }

    public ImageInfo Info { get; }

    private Bitmap? bitmap;
    private bool cpuSynced;

    private bool isDisposed;

    private Paint nearestNeighborReplacingPaint =
        new() { BlendMode = BlendMode.Src, FilterQuality = FilterQuality.None };

    public Texture(VecI size)
        : this(new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgb())
        {
            GpuBacked = true
        })
    {
    }

    private void OnChanged(RectD? changedRect)
    {
        cpuSynced = false;
    }

    public static Texture ForDisplay(VecI size)
    {
        return new Texture(
            new ImageInfo(size.X, size.Y, ColorType.Rgba8888, AlphaType.Premul, ColorSpace.CreateSrgb())
            {
                GpuBacked = true
            });
    }

    public static Texture ForProcessing(VecI size)
    {
        return new Texture(
            new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgbLinear())
            {
                GpuBacked = true
            });
    }

    public static Texture ForProcessing(VecI size, ColorSpace colorSpace)
    {
        return new Texture(
            new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, colorSpace) { GpuBacked = true });
    }

    public static Texture ForProcessing(DrawingSurface copySizeAndMatrixFrom, ColorSpace colorSpace)
    {
        Texture tex = new Texture(
            new ImageInfo(
                copySizeAndMatrixFrom.DeviceClipBounds.Size.X + copySizeAndMatrixFrom.DeviceClipBounds.Pos.X,
                copySizeAndMatrixFrom.DeviceClipBounds.Size.Y + copySizeAndMatrixFrom.DeviceClipBounds.Pos.Y,
                ColorType.RgbaF16, AlphaType.Premul, colorSpace) { GpuBacked = true });
        tex.DrawingSurface.Canvas.SetMatrix(copySizeAndMatrixFrom.Canvas.TotalMatrix);

        return tex;
    }

    public Texture(ImageInfo imageInfo)
    {
        Info = imageInfo;
        Size = new VecI(imageInfo.Width, imageInfo.Height);
        if (!imageInfo.GpuBacked)
            throw new ArgumentException(
                "Textures are GPU backed, add GpuBacked = true or use Surface for CPU backed surfaces.");

        ColorSpace = imageInfo.ColorSpace;

        DrawingBackendApi.Current.RenderingDispatcher.Invoke(
            () =>
                DrawingSurface =
                    DrawingSurface.Create(imageInfo)
        );

        DrawingSurface.Changed += DrawingSurfaceOnChanged;
        Changed += OnChanged;
    }

    public Texture(Texture other) : this(other.Size)
    {
        using var ctx = EnsureContext();
        DrawingSurface.Canvas.DrawSurface(other.DrawingSurface, 0, 0);
    }

    internal Texture(DrawingSurface drawingSurface)
    {
        DrawingSurface = drawingSurface;
        Size = drawingSurface.DeviceClipBounds.Size;
        DrawingSurface.Changed += DrawingSurfaceOnChanged;
    }

    public object Clone()
    {
        return new Texture(this);
    }

    private void DrawingSurfaceOnChanged(RectD? changedRect)
    {
        Changed?.Invoke(changedRect);
    }


    public static Texture Load(string path)
    {
        using var ctx = EnsureContext();
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);
        using var image = Image.FromEncodedData(path);
        if (image is null)
            throw new ArgumentException($"The image with path {path} couldn't be loaded");

        Texture texture = new Texture(image.Size);
        texture.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return texture;
    }

    public static Texture Load(byte[] data)
    {
        using var ctx = EnsureContext();
        using Image image = Image.FromEncodedData(data);

        if (image is null || image.Size.ShortestAxis <= 0)
            throw new ArgumentException("The image couldn't be loaded");

        Texture texture = new Texture(image.Size);
        texture.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return texture;
    }

    public static Texture? Load(byte[] encoded, ColorType colorType, VecI imageSize)
    {
        using var ctx = EnsureContext();
        using var image = Image.FromPixels(new ImageInfo(imageSize.X, imageSize.Y, colorType), encoded);
        if (image is null)
            return null;

        var surface = new Texture(new VecI(image.Width, image.Height));
        surface.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return surface;
    }

    public Texture CreateResized(VecI newSize, ResizeMethod method)
    {
        using var ctx = EnsureContext();
        using Image image = DrawingSurface.Snapshot();
        Texture newTexture = new(newSize);
        using Paint paint = new();

        FilterQuality filterQuality = method switch
        {
            ResizeMethod.HighQuality => FilterQuality.High,
            ResizeMethod.MediumQuality => FilterQuality.Medium,
            ResizeMethod.LowQuality => FilterQuality.Low,
            _ => FilterQuality.None
        };

        paint.FilterQuality = filterQuality;

        newTexture.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y), paint);

        return newTexture;
    }

    public void CopyTo(Texture destination)
    {
        destination.DrawingSurface.Canvas.DrawSurface(DrawingSurface, 0, 0);
    }

    public void DrawBytes(VecI surfaceSize, byte[] pixels, ColorType color, AlphaType alphaType)
    {
        if (surfaceSize != Size)
            throw new ArgumentException("Surface size must match the size of the byte array");

        using Image image = Image.FromPixels(new ImageInfo(Size.X, Size.Y, color, alphaType, ColorSpace.CreateSrgb()),
            pixels);
        DrawingSurface.Canvas.DrawImage(image, 0, 0);
    }

    public Texture ResizeNearestNeighbor(VecI newSize)
    {
        using Image image = DrawingSurface.Snapshot();
        Texture newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y),
            nearestNeighborReplacingPaint);
        return newSurface;
    }

    public Texture Resize(VecI newSize, FilterQuality quality)
    {
        using var ctx = EnsureContext();
        using Image image = DrawingSurface.Snapshot();
        using Paint paint = new();
        paint.FilterQuality = quality;
        Texture newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y),
            paint);
        return newSurface;
    }

    public Color GetSRGBPixel(VecI vecI)
    {
        var color = GetPixel(vecI);
        if (color is { R: 0, G: 0, B: 0, A: 0 })
            return Color.Empty;

        if (!ColorSpace.IsSrgb)
        {
            var transformFunction = ColorSpace.CreateSrgb().GetTransformFunction();
            return (Color)color.TransformColor(transformFunction.Invert());
        }

        return color;
    }

    public Color GetPixel(VecI at)
    {
        if (at.X < 0 || at.X >= Size.X || at.Y < 0 || at.Y >= Size.Y)
            return Color.Empty;

        SyncBitmap();

        return bitmap.PeekPixels().GetPixelColor(at);
    }

    private void SyncBitmap()
    {
        if (!cpuSynced)
        {
            if (bitmap == null)
            {
                bitmap = new Bitmap(Info);
            }

            DrawingSurface.ReadPixels(Info, bitmap.Address, bitmap.Info.RowBytes, 0, 0);

            cpuSynced = true;
        }
    }

    public void AddDirtyRect(RectI dirtyRect)
    {
        Changed?.Invoke(new RectD(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height));
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        DrawingSurface.Changed -= DrawingSurfaceOnChanged;
        DrawingSurface.Dispose();
        bitmap?.Dispose();
        nearestNeighborReplacingPaint.Dispose();
    }

    public static Texture FromExisting(DrawingSurface drawingSurface)
    {
        Texture texture = new(drawingSurface);
        return texture;
    }

    private static IDisposable EnsureContext()
    {
        return DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
    }
}
