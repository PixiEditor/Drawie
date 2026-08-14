using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Core;

public class NativeTexture : IDisposable, ICloneable, IPixelsMap, IFramebufferInfo, ITexture
{
    public VecI Size { get; }
    public DrawingSurface DrawingSurface { get; private set; }

    public event SurfaceChangedEventHandler? Changed;

    public bool IsDisposed => isDisposed || DrawingSurface.IsDisposed;
    public bool IsHardwareAccelerated { get; } = DrawingBackendApi.Current.IsHardwareAccelerated;

    public ColorSpace ColorSpace { get; }

    public ImageInfo ImageInfo { get; }
    public ulong TextureId => FramebufferId;

    private DrawingSurface? cpuSurface;
    private Pixmap? cpuPixmap;
    private bool cpuSynced;

    private bool isDisposed;
    private bool disposePending;

    private HashSet<object> lockDisposes = new();

    private Paint nearestNeighborReplacingPaint =
        new() { BlendMode = BlendMode.Src };

    public NativeTexture(VecI size)
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

    public static NativeTexture ForDisplay(VecI size)
    {
        return new NativeTexture(
            new ImageInfo(size.X, size.Y, ColorType.Rgba8888, AlphaType.Premul, ColorSpace.CreateSrgb())
            {
                GpuBacked = true
            });
    }

    public static NativeTexture ForProcessing(VecI size)
    {
        return new NativeTexture(
            new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, ColorSpace.CreateSrgbLinear())
            {
                GpuBacked = true
            });
    }

    public static NativeTexture ForProcessing(VecI size, ColorSpace colorSpace)
    {
        return new NativeTexture(
            new ImageInfo(size.X, size.Y, ColorType.RgbaF16, AlphaType.Premul, colorSpace) { GpuBacked = true });
    }

    public static NativeTexture? ForProcessing(DrawingSurface copySizeAndMatrixFrom, ColorSpace colorSpace)
    {
        VecI size = new VecI(
            copySizeAndMatrixFrom.DeviceClipBounds.Size.X + copySizeAndMatrixFrom.DeviceClipBounds.Pos.X,
            copySizeAndMatrixFrom.DeviceClipBounds.Size.Y + copySizeAndMatrixFrom.DeviceClipBounds.Pos.Y);
        if (size.X <= 0 || size.Y <= 0)
            return null;

        NativeTexture tex = new NativeTexture(
            new ImageInfo(size.X, size.Y,
                ColorType.RgbaF16, AlphaType.Premul, colorSpace) { GpuBacked = true });
        tex.DrawingSurface.Canvas.SetMatrix(copySizeAndMatrixFrom.Canvas.TotalMatrix);

        return tex;
    }

    public static NativeTexture ForProcessing(Canvas copySizeAndMatrixFrom, ColorSpace colorSpace)
    {
        NativeTexture tex = new NativeTexture(
            new ImageInfo(
                copySizeAndMatrixFrom.DeviceClipBounds.Size.X + copySizeAndMatrixFrom.DeviceClipBounds.Pos.X,
                copySizeAndMatrixFrom.DeviceClipBounds.Size.Y + copySizeAndMatrixFrom.DeviceClipBounds.Pos.Y,
                ColorType.RgbaF16, AlphaType.Premul, colorSpace) { GpuBacked = true });
        tex.DrawingSurface.Canvas.SetMatrix(copySizeAndMatrixFrom.TotalMatrix);

        return tex;
    }

    public NativeTexture(ImageInfo imageImageInfo)
    {
        Size = new VecI(imageImageInfo.Width, imageImageInfo.Height);
        if (!imageImageInfo.GpuBacked)
            throw new ArgumentException(
                "Textures are GPU backed, add GpuBacked = true or use Surface for CPU backed surfaces.");

        ColorSpace = imageImageInfo.ColorSpace;

        DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
            {
                DrawingSurface = DrawingSurface.Create(imageImageInfo);
                if (DrawingSurface == null)
                {
                    imageImageInfo.GpuBacked = false;
                    DrawingSurface = DrawingSurface.Create(imageImageInfo);
                    if (DrawingSurface == null)
                    {
                        throw new Exception("Could not create DrawingSurface for Texture.");
                    }
                }
                
                DrawingSurface.Changed += DrawingSurfaceOnChanged;
            }
        );

        ImageInfo = imageImageInfo;
        Changed += OnChanged;
    }

    public NativeTexture(NativeTexture other) : this(other.Size)
    {
        using var ctx = EnsureContext();
        DrawingSurface.Canvas.DrawSurface(other.DrawingSurface, 0, 0);
    }

    internal NativeTexture(DrawingSurface drawingSurface)
    {
        DrawingSurface = drawingSurface;
        Size = drawingSurface.DeviceClipBounds.Size;
        DrawingSurface.Changed += DrawingSurfaceOnChanged;
    }

    public object Clone()
    {
        return new NativeTexture(this);
    }

    private void DrawingSurfaceOnChanged(RectD? changedRect)
    {
        Changed?.Invoke(changedRect);
    }


    public static NativeTexture Load(string path)
    {
        using var ctx = EnsureContext();
        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);
        using var image = Image.FromEncodedData(path);
        if (image is null)
            throw new ArgumentException($"The image with path {path} couldn't be loaded");

        NativeTexture nativeTexture = new NativeTexture(image.Size);
        nativeTexture.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return nativeTexture;
    }

    public static NativeTexture Load(byte[] data)
    {
        using var ctx = EnsureContext();
        using Image image = Image.FromEncodedData(data);

        if (image is null || image.Size.ShortestAxis <= 0)
            throw new ArgumentException("The image couldn't be loaded");

        NativeTexture nativeTexture = new NativeTexture(image.Size);
        nativeTexture.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return nativeTexture;
    }

    public static NativeTexture? Load(byte[] encoded, ColorType colorType, VecI imageSize)
    {
        using var ctx = EnsureContext();
        using var image = Image.FromPixels(new ImageInfo(imageSize.X, imageSize.Y, colorType), encoded);
        if (image is null)
            return null;

        var surface = new NativeTexture(new VecI(image.Width, image.Height));
        surface.DrawingSurface.Canvas.DrawImage(image, 0, 0);

        return surface;
    }

    public NativeTexture CreateResized(VecI newSize, ResizeMethod method)
    {
        using var ctx = EnsureContext();
        using Image image = DrawingSurface.Snapshot();
        NativeTexture newNativeTexture = new(newSize);
        using Paint paint = new();

        FilterQuality filterQuality = method switch
        {
            ResizeMethod.HighQuality => FilterQuality.High,
            ResizeMethod.MediumQuality => FilterQuality.Medium,
            ResizeMethod.LowQuality => FilterQuality.Low,
            _ => FilterQuality.None
        };

        newNativeTexture.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y), paint);

        return newNativeTexture;
    }

    public void CopyTo(NativeTexture destination)
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

    public NativeTexture ResizeNearestNeighbor(VecI newSize)
    {
        using Image image = DrawingSurface.Snapshot();
        NativeTexture newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y),
            nearestNeighborReplacingPaint);
        return newSurface;
    }

    public NativeTexture Resize(VecI newSize, FilterQuality quality)
    {
        using var ctx = EnsureContext();
        using Image image = DrawingSurface.Snapshot();
        using Paint paint = new();
        NativeTexture newSurface = new(newSize);
        newSurface.DrawingSurface.Canvas.DrawImage(image, new RectD(0, 0, newSize.X, newSize.Y),
            paint);
        return newSurface;
    }

    public Color GetSrgbPixel(VecI vecI)
    {
        var color = GetRawPixel(vecI);
        if (color is { R: 0, G: 0, B: 0, A: 0 })
            return Color.Empty;

        if (!ColorSpace.IsSrgb)
        {
            var transformFunction = ColorSpace.CreateSrgb().GetTransformFunction();
            return (Color)color.TransformColor(transformFunction.Invert());
        }

        return color;
    }

    public Color GetRawPixel(VecI at)
    {
        if (at.X < 0 || at.X >= Size.X || at.Y < 0 || at.Y >= Size.Y)
            return Color.Empty;

        using var ctx = EnsureContext();
        SyncBitmap();

        var pixmap = cpuPixmap ??= cpuSurface.PeekPixels();
        return pixmap.GetPixelColor(at);
    }


    public ColorF GetRawPixelPrecise(VecI pos)
    {
        if (pos.X < 0 || pos.X >= Size.X || pos.Y < 0 || pos.Y >= Size.Y)
            return Color.Empty;

        using var ctx = EnsureContext();
        SyncBitmap();

        var pixmap = cpuPixmap ??= cpuSurface.PeekPixels();
        return pixmap.GetPixelColorPrecise(pos);
    }


    public Pixmap PeekPixels()
    {
        SyncBitmap();
        return cpuSurface.PeekPixels();
    }

    void IPixelsMap.MarkPixelsChanged()
    {
        if (isDisposed)
            throw new ObjectDisposedException("Texture");

        if (cpuSurface == null)
            return;

        using var ctx = EnsureContext();
        using Paint srcPaint = new() { BlendMode = BlendMode.Src };
        DrawingSurface.Canvas.DrawSurface(cpuSurface, 0, 0, srcPaint);
        cpuSynced = true;
    }

    private void SyncBitmap()
    {
        if (!cpuSynced)
        {
            if (cpuSurface == null)
            {
                cpuSurface = DrawingSurface.Create(ImageInfo with { GpuBacked = false });
            }

            using var ctx = EnsureContext();
            using Paint srcPaint = new() { BlendMode = BlendMode.Src };
            cpuSurface.Canvas.DrawSurface(DrawingSurface, 0, 0, srcPaint);
            cpuPixmap?.Dispose();
            cpuPixmap = null;

            cpuSynced = true;
        }
    }

    public void AddDirtyRect(RectD? dirtyRect)
    {
        Changed?.Invoke(dirtyRect);
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        if (lockDisposes.Count > 0)
        {
            disposePending = true;
            return;
        }

        using var ctx = EnsureContext();
        isDisposed = true;
        DrawingSurface.Changed -= DrawingSurfaceOnChanged;
        DrawingSurface.Dispose();
        cpuSurface?.Dispose();
        cpuPixmap?.Dispose();
        nearestNeighborReplacingPaint.Dispose();
    }

    public static NativeTexture FromExisting(DrawingSurface drawingSurface)
    {
        NativeTexture nativeTexture = new(drawingSurface);
        return nativeTexture;
    }

    private static IDisposable EnsureContext()
    {
        return DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
    }

    public void LockDispose(object locker)
    {
        if (lockDisposes.Contains(locker) || isDisposed)
            return;

        lockDisposes.Add(locker);
    }

    public void UnlockDispose(object locker)
    {
        if (!lockDisposes.Contains(locker))
            return;

        lockDisposes.Remove(locker);

        if (lockDisposes.Count == 0 && disposePending)
        {
            Dispose();
        }
    }

    public unsafe bool IsFullyTransparent()
    {
        ulong* ptr = (ulong*)PeekPixels().GetPixels();
        for (int i = 0; i < Size.X * Size.Y; i++)
        {
            // ptr[i] actually contains 4 16-bit floats. We only care about the first one which is alpha.
            // An empty pixel can have alpha of 0 or -0 (not sure if -0 actually ever comes up). 0 in hex is 0x0, -0 in hex is 0x8000
            if ((ptr[i] & 0x1111_0000_0000_0000) != 0 && (ptr[i] & 0x1111_0000_0000_0000) != 0x8000_0000_0000_0000)
                return false;
        }

        return true;
    }

#if DEBUG
    public void SaveToDesktop()
    {
        using Surface surf = Surface.ForDisplay(Size);
        surf.DrawingSurface.Canvas.DrawSurface(DrawingSurface, 0, 0);
        surf.SaveToDesktop();
    }
#endif

    public ulong FramebufferId =>
        DrawingBackendApi.Current.SurfaceImplementation.GetFramebufferInfo(DrawingSurface.ObjectPointer)
            ?.FramebufferId ?? throw new Exception("Framebuffer info not available.");
}