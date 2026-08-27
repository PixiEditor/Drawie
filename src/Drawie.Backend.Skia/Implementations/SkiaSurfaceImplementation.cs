using System.Diagnostics;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Textures;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaSurfaceImplementation : SkObjectImplementation<SKSurface>, ISurfaceImplementation
    {
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private readonly SkiaCanvasImplementation _canvasImplementation;
        private readonly SkiaPaintImplementation _paintImplementation;

        private Dictionary<SKSurface, INativeSurfaceInfo> nativeSurfaceInfos =
            new Dictionary<SKSurface, INativeSurfaceInfo>();

        internal GRContext? GrContext { get; set; }
        internal IGraphicsDevice GraphicsDevice { get; set; }

        private readonly SurfaceOrigin defaultSurfaceOrigin;

        private HashSet<IntPtr> surfacesInUse = new HashSet<IntPtr>();

        public SkiaSurfaceImplementation(GRContext context, SurfaceOrigin surfaceOrigin,
            SkiaPixmapImplementation pixmapImplementation,
            SkiaCanvasImplementation canvasImplementation, SkiaPaintImplementation paintImplementation)
        {
            _pixmapImplementation = pixmapImplementation;
            _canvasImplementation = canvasImplementation;
            _paintImplementation = paintImplementation;
            GrContext = context;
            defaultSurfaceOrigin = surfaceOrigin;
        }

        public Pixmap PeekPixels(DrawingSurface drawingSurface)
        {
            SKPixmap pixmap = this[drawingSurface.ObjectPointer].PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public bool ReadPixels(DrawingSurface drawingSurface, ImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes,
            int srcX,
            int srcY)
        {
            return this[drawingSurface.ObjectPointer]
                .ReadPixels(dstInfo.ToSkImageInfo(), dstPixels, dstRowBytes, srcX, srcY);
        }

        public void Draw(DrawingSurface drawingSurface, Canvas surfaceToDraw, int x, int y, Paint drawingPaint)
        {
            SKCanvas canvas = _canvasImplementation[surfaceToDraw.ObjectPointer];
            SKPaint paint = _paintImplementation[drawingPaint.ObjectPointer];
            var instance = this[drawingSurface.ObjectPointer];
            instance.Draw(canvas, x, y, paint);
        }

        public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixels, int rowBytes)
        {
            SKSurface? skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked, pixels, rowBytes);
            return CreateDrawingSurface(skSurface);
        }

        public DrawingSurface? Create(ImageInfo imageInfo, IntPtr pixelBuffer)
        {
            SKImageInfo info = imageInfo.ToSkImageInfo();
            SKSurface? skSurface = CreateSkiaSurface(info, imageInfo.GpuBacked, pixelBuffer);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels, int rowBytes)
        {
            if (isGpuBacked)
            {
                SKSurface? skSurface = CreateSkiaSurface(imageInfo, true);
                if (skSurface == null)
                {
                    return null;
                }

                using var image = SKImage.FromPixelCopy(imageInfo, pixels, rowBytes);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels, rowBytes);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo imageInfo, bool isGpuBacked, IntPtr pixels)
        {
            if (isGpuBacked)
            {
                SKSurface? skSurface = CreateSkiaSurface(imageInfo, true);
                if (skSurface == null)
                {
                    return null;
                }

                using var image = SKImage.FromPixelCopy(imageInfo, pixels);

                var canvas = skSurface.Canvas;
                canvas.DrawImage(image, new SKPoint(0, 0));

                return skSurface;
            }

            return SKSurface.Create(imageInfo, pixels);
        }

        public DrawingSurface? Create(Pixmap pixmap)
        {
            SKPixmap skPixmap = _pixmapImplementation[pixmap.ObjectPointer];
            var skSurface = CreateSkiaSurface(skPixmap);

            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKPixmap skPixmap)
        {
            SKSurface skSurface = SKSurface.Create(skPixmap);
            return skSurface;
        }

        public DrawingSurface? Create(ImageInfo imageInfo)
        {
            SKSurface? skSurface = CreateSkiaSurface(imageInfo.ToSkImageInfo(), imageInfo.GpuBacked);
            return CreateDrawingSurface(skSurface);
        }

        private SKSurface? CreateSkiaSurface(SKImageInfo info, bool gpu)
        {
            if (!gpu || GrContext == null)
            {
                return SKSurface.Create(info);
            }

            if (GraphicsDevice == null)
            {
                throw new InvalidOperationException("GraphicsDevice is not initialized.");
            }

            var texture = GraphicsDevice.CreateTexture(new TextureDesc()
            {
                Format = TextureFormat.RGBA8_Unorm, Width = info.Width, Height = info.Height
            });

            var surface = CreateFromNativeTexture(texture, new VecI(info.Width, info.Height), defaultSurfaceOrigin,
                false,
                out var framebufferInfo);
            if (surface == null) return null;

            nativeSurfaceInfos[surface] = framebufferInfo;
            return surface;
        }

        internal SKSurface? CreateFromNativeTexture(ITexture renderTexture, VecI size, SurfaceOrigin surfaceOrigin,
            bool asRenderTarget,
            out INativeSurfaceInfo fbInfo)
        {
            if (renderTexture is IVkTexture texture)
            {
                var imageInfo = new GRVkImageInfo()
                {
                    CurrentQueueFamily = texture.QueueFamily,
                    Format = texture.ImageFormat,
                    Image = texture.ImageHandle,
                    ImageLayout = texture.Layout,
                    ImageTiling = texture.Tiling,
                    ImageUsageFlags = texture.UsageFlags,
                    LevelCount = 1,
                    SampleCount = 1,
                    Protected = false,
                    SharingMode = texture.TargetSharingMode,
                };

                var backendRenderTarget = new GRBackendRenderTarget(size.X, size.Y, imageInfo);
                var surface = SKSurface.Create(GrContext, backendRenderTarget, (GRSurfaceOrigin)surfaceOrigin,
                    SKColorType.Rgba8888, new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));

                fbInfo = new SkiaNativeSurfaceInfo(backendRenderTarget, imageInfo);
                return surface;
            }

            if (renderTexture is IWebGlTexture or IOpenGlTexture)
            {
                uint textureId = renderTexture switch
                {
                    IWebGlTexture wgl => wgl.TextureId,
                    IOpenGlTexture ogl => (uint)ogl.TextureId,
                    _ => throw new ArgumentException("Unsupported texture type.")
                };

                SKSurface? surface;
                if (!asRenderTarget)
                {
                    const uint OpenGlTexture2D = 3553;
                    const uint RGBA8 = 0x8058;
                    var info = new GRGlTextureInfo(OpenGlTexture2D, textureId, RGBA8);
                    var backendRenderTarget = new GRBackendTexture(size.X, size.Y, false, info);

                    surface = SKSurface.Create(GrContext, backendRenderTarget, (GRSurfaceOrigin)surfaceOrigin,
                        SKColorType.Rgba8888);
                    fbInfo = new SkiaNativeSurfaceInfo(backendRenderTarget, info);
                }
                else
                {
                    GRGlFramebufferInfo grGlFramebufferInfo =
                        new GRGlFramebufferInfo(textureId, SKColorType.Rgba8888.ToGlSizedFormat());
                    GRBackendRenderTarget backendRenderTarget = new GRBackendRenderTarget(size.X, size.Y, 1, 0,
                        grGlFramebufferInfo);

                    surface = SKSurface.Create(GrContext, backendRenderTarget, (GRSurfaceOrigin)surfaceOrigin,
                        SKColorType.Rgba8888);

                    fbInfo = new SkiaNativeSurfaceInfo(backendRenderTarget, grGlFramebufferInfo);
                }

                return surface;
            }

            throw new ArgumentException("Unsupported texture type.");
        }

        public void Dispose(DrawingSurface drawingSurface)
        {
            var instance = this.GetInstanceOrDefault(drawingSurface.ObjectPointer);
            ulong? surfaceId = null;
            if (instance != null)
            {
                nativeSurfaceInfos.Remove(instance, out var surfaceInfo);
                surfaceId = surfaceInfo?.SurfaceId;
            }

            UnmanageAndDispose(drawingSurface.ObjectPointer);
            if (surfaceId != null)
            {
                throw new NotImplementedException("Implement a proper lifetime management for GPU-backed surfaces.");
                GraphicsDevice.DisposeTexture(surfaceId.Value);
            }
        }

        public object GetNativeSurface(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        private DrawingSurface? CreateDrawingSurface(SKSurface? skSurface)
        {
            if (skSurface == null)
            {
                return null;
            }

#if DRAWIE_TRACE
            Trace(skSurface);
#endif

            IntPtr canvasHandle = _canvasImplementation.AddManagedInstance(skSurface.Canvas);
            Canvas canvas = new Canvas(canvasHandle);

            IntPtr surfaceHandle = AddManagedInstance(skSurface);
            DrawingSurface surface = new DrawingSurface(surfaceHandle, canvas);

            return surface;
        }

        public void Flush(DrawingSurface drawingSurface)
        {
            this[drawingSurface.ObjectPointer].Flush(true, true);
        }

        public DrawingSurface? FromNative(object native)
        {
            if (native is not SKSurface skSurface)
            {
                throw new ArgumentException("Native object is not of type SKSurface");
            }

            return CreateDrawingSurface(skSurface);
        }

        public RectI GetDeviceClipBounds(IntPtr drawingSurface)
        {
            SKRectI skRectI = this[drawingSurface].Canvas.DeviceClipBounds;
            return new RectI(skRectI.Left, skRectI.Top, skRectI.Width, skRectI.Height);
        }

        public void Unmanage(DrawingSurface surface)
        {
            Unmanage(surface.ObjectPointer);
        }

        public RectD GetLocalClipBounds(IntPtr objectPointer)
        {
            SKRect skRect = this[objectPointer].Canvas.LocalClipBounds;
            return new RectD(skRect.Left, skRect.Top, skRect.Width, skRect.Height);
        }

        public INativeSurfaceInfo? GetNativeSurfaceInfo(IntPtr objectPointer)
        {
            return nativeSurfaceInfos.GetValueOrDefault(this[objectPointer]);
        }

        public void AddManagedFramebuffer(SKSurface nativeHandle, INativeSurfaceInfo fbInfo)
        {
            nativeSurfaceInfos.Add(nativeHandle, fbInfo);
        }
    }
}
