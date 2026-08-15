using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Skia.Encoders;
using Drawie.Skia.Extensions;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageImplementation : SkObjectImplementation<SKImage>, IImageImplementation
    {
        private readonly SkObjectImplementation<SKData> _imgImplementation;
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private SkiaSurfaceImplementation _surfaceImplementation;
        private SkiaShaderImplementation shaderImpl;
        /*
        private Dictionary<IntPtr, ITexture> textureInfos = new Dictionary<IntPtr, ITexture>();
        */

        private Dictionary<EncodedImageFormat, IImageEncoder> nonSkiaEncoders = new Dictionary<EncodedImageFormat, IImageEncoder>()
        {
            { EncodedImageFormat.Bmp , new BmpEncoder() }
        };

        public SkiaImageImplementation(SkObjectImplementation<SKData> imgDataImplementation,
            SkiaPixmapImplementation pixmapImplementation, SkiaShaderImplementation shaderImplementation)
        {
            _imgImplementation = imgDataImplementation;
            _pixmapImplementation = pixmapImplementation;
            shaderImpl = shaderImplementation;
        }

        public void SetSurfaceImplementation(SkiaSurfaceImplementation surfaceImplementation)
        {
            _surfaceImplementation = surfaceImplementation;
        }

        public Image Snapshot(DrawingSurface drawingSurface)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];
            SKImage snapshot = surface.Snapshot();

            AddManagedInstance(snapshot);
            return new Image(snapshot.Handle);
        }

        public Image Snapshot(DrawingSurface drawingSurface, RectI bounds)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];
            SKImage snapshot = surface.Snapshot(bounds.ToSkRectI());

            AddManagedInstance(snapshot);
            return new Image(snapshot.Handle);
        }

        /*public Image? TextureSnapshot(DrawingSurface drawingSurface)
        {
            var surface = _surfaceImplementation![drawingSurface.ObjectPointer];

            VecI size = new VecI(surface.Canvas.DeviceClipBounds.Width, surface.Canvas.DeviceClipBounds.Height);
            
            /*
            var fbInfo = DrawingBackendApi.Current.SurfaceImplementation.GetFramebufferInfo(drawingSurface.ObjectPointer);

            if (fbInfo is not SkiaFramebufferInfo skiaFramebufferInfo)
            {
                return null;
            }
            
            SKImage? snapshot = CreateFromFramebuffer(skiaFramebufferInfo, size, SurfaceOrigin.BottomLeft, out var textureInfo);
            if (snapshot == null) return null;#1#

            var nativeTexture =_surfaceImplementation.GraphicsDevice.CreateTexture(new TextureDesc()
            {
                Width = size.X,
                Height = size.Y,
                Format = TextureFormat.RGBA8_Unorm
            });

            SKImage snapshot = SnapshotToOwnedTexture(surface, (uint)nativeTexture.TextureId, size.X, size.Y,
                SurfaceOrigin.BottomLeft, out var textureInfo);

            AddManagedInstance(snapshot);
            textureInfos[snapshot.Handle] = textureInfo;
            return new Image(snapshot.Handle);
        }
        
        internal SKImage? SnapshotToOwnedTexture(
            SKSurface source,
            uint textureId,
            int width,
            int height,
            SurfaceOrigin origin, out SkiaTextureInfo info)
        {
            const uint GL_TEXTURE_2D = 3553;
            const uint GL_RGBA8 = 0x8058;

            var textureInfo = new GRGlTextureInfo(
                GL_TEXTURE_2D,
                textureId,
                GL_RGBA8);

            var backendTexture = new GRBackendTexture(
                width,
                height,
                false,
                textureInfo);

            var targetSurface = SKSurface.Create(
                _surfaceImplementation.GrContext,
                backendTexture,
                (GRSurfaceOrigin)origin,
                SKColorType.Rgba8888);

            info = new SkiaTextureInfo(backendTexture);
            
            if (targetSurface == null)
                return null;

            using var snapshot = source.Snapshot();

            targetSurface.Canvas.DrawImage(snapshot, 0, 0, SKSamplingOptions.Default);
            
            return SKImage.FromTexture(
                _surfaceImplementation.GrContext,
                backendTexture,
                (GRSurfaceOrigin)origin,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);
        }*/
        
        internal SKImage? CreateFromFramebuffer(SkiaNativeSurfaceInfo skiaNativeSurfaceInfo, VecI size, SurfaceOrigin surfaceOrigin, out ITexture fbInfo)
        {
            if (skiaNativeSurfaceInfo.VkImageInfo != null)
            {
                var imageInfo = skiaNativeSurfaceInfo.VkImageInfo.Value;
                var backendRenderTarget = new GRBackendTexture(size.X, size.Y, imageInfo);
                var surface = SKImage.FromTexture(_surfaceImplementation.GrContext, backendRenderTarget,
                    (GRSurfaceOrigin)surfaceOrigin, SKColorType.Rgba8888, SKAlphaType.Premul);

                fbInfo = new SkiaTextureInfo(backendRenderTarget);
                return surface;
            }

            if (skiaNativeSurfaceInfo.GlFramebufferInfo != null)
            {
                uint textureId = skiaNativeSurfaceInfo.GlFramebufferInfo.Value.FramebufferObjectId;

                const uint OpenGlTexture2D = 3553;
                const uint RGBA8 = 0x8058;
                GRBackendTexture backendRenderTarget =
                    new GRBackendTexture(size.X, size.Y, false, new GRGlTextureInfo(OpenGlTexture2D, textureId, RGBA8));
                
                var surface = SKImage.FromTexture(_surfaceImplementation.GrContext, backendRenderTarget, (GRSurfaceOrigin)surfaceOrigin,
                    SKColorType.Rgba8888, SKAlphaType.Premul);

                fbInfo = new SkiaTextureInfo(backendRenderTarget);
                return surface;
            }

            throw new ArgumentException("Unsupported texture type.");
        }

        public Image? FromEncodedData(byte[] dataBytes)
        {
            SKImage img = SKImage.FromEncodedData(dataBytes);
            if (img is null)
                return null;
            AddManagedInstance(img);

            return new Image(img.Handle);
        }

        public void DisposeImage(Image image)
        {
            UnmanageAndDispose(image.ObjectPointer);
            /*
            textureInfos.Remove(image.ObjectPointer);
        */
        }

        public Image? FromEncodedData(string path)
        {
            var nativeImg = SKImage.FromEncodedData(path);
            if (nativeImg is null)
                return null;
            AddManagedInstance(nativeImg);
            return new Image(nativeImg.Handle);
        }

        public Image? FromPixelCopy(ImageInfo info, byte[] pixels)
        {
            var nativeImg = SKImage.FromPixelCopy(info.ToSkImageInfo(), pixels);
            if (nativeImg is null)
                return null;
            AddManagedInstance(nativeImg);
            return new Image(nativeImg.Handle);
        }

        public Pixmap PeekPixels(Image image)
        {
            var native = this[image.ObjectPointer];
            var pixmap = native.PeekPixels();
            return _pixmapImplementation.CreateFrom(pixmap);
        }

        public void GetColorShifts(ref int platformColorAlphaShift, ref int platformColorRedShift,
            ref int platformColorGreenShift,
            ref int platformColorBlueShift)
        {
            platformColorAlphaShift = SKImageInfo.PlatformColorAlphaShift;
            platformColorRedShift = SKImageInfo.PlatformColorRedShift;
            platformColorGreenShift = SKImageInfo.PlatformColorGreenShift;
            platformColorBlueShift = SKImageInfo.PlatformColorBlueShift;
        }

        public ImgData Encode(Image image)
        {
            var native = this[image.ObjectPointer];
            var encoded = native.Encode();
            _imgImplementation.AddManagedInstance(encoded);
            return new ImgData(encoded.Handle);
        }

        public ImgData Encode(Image image, EncodedImageFormat format, int quality)
        {
            var native = this[image.ObjectPointer];
            SKData? encoded = null;
            if (format != EncodedImageFormat.Png && format != EncodedImageFormat.Jpeg &&
                format != EncodedImageFormat.Webp)
            {
                if (nonSkiaEncoders.TryGetValue(format, out var encoder))
                {
                    byte[] bytes = encoder.Encode(image);
                    encoded = SKData.CreateCopy(bytes);
                }
                else
                {
                    throw new NotSupportedException($"Encoding {format} format is not supported");
                }
            }
            else
            {
                encoded = native.Encode((SKEncodedImageFormat)format, quality);
            }

            _imgImplementation.AddManagedInstance(encoded);
            return new ImgData(encoded.Handle);
        }

        public int GetWidth(IntPtr objectPointer)
        {
            return this[objectPointer].Width;
        }

        public int GetHeight(IntPtr objectPointer)
        {
            return this[objectPointer].Height;
        }

        public Image Clone(Image image)
        {
            var native = this[image.ObjectPointer];
            var encoded = native.Encode();
            var clone = SKImage.FromEncodedData(encoded);
            AddManagedInstance(clone);
            return new Image(clone.Handle);
        }

        public Pixmap PeekPixels(IntPtr objectPointer)
        {
            var nativePixmap = this[objectPointer].PeekPixels();

            return _pixmapImplementation.CreateFrom(nativePixmap);
        }

        public ImageInfo GetImageInfo(IntPtr objectPointer)
        {
            var info = this[objectPointer].Info;
            return info.ToImageInfo();
        }

        public Shader ToShader(IntPtr objectPointer)
        {
            var shader = this[objectPointer].ToShader();
            shaderImpl.AddManagedInstance(shader);
            return new Shader(shader.Handle);
        }


        public Shader ToShader(IntPtr objectPointer, TileMode tileX, TileMode tileY, SamplingOptions samplingOptions, Matrix3X3 localMatrix)
        {
            var shader = this[objectPointer]
                .ToShader((SKShaderTileMode)tileX, (SKShaderTileMode)tileY, samplingOptions.ToSkSamplingOptions(), localMatrix.ToSkMatrix());
            shaderImpl.AddManagedInstance(shader);
            return new Shader(shader.Handle);
        }

        public Shader ToRawShader(IntPtr objectPointer)
        {
            var shader = this[objectPointer].ToRawShader();
            shaderImpl.AddManagedInstance(shader);
            return new Shader(shader.Handle);
        }

        public Shader? ToShader(IntPtr objectPointer, TileMode clamp, TileMode tileMode, Matrix3X3 fillMatrixValue)
        {
            var shader = this[objectPointer].ToShader((SKShaderTileMode)clamp, (SKShaderTileMode)tileMode,
                fillMatrixValue.ToSkMatrix());
            if (shader is null)
                return null;

            shaderImpl.AddManagedInstance(shader);
            return new Shader(shader.Handle);
        }

        public uint GetUniqueId(IntPtr objectPointer)
        {
            return this[objectPointer].UniqueId;
        }

        /*
        public ulong? GetTextureId(IntPtr objectPointer)
        {
            if (textureInfos.TryGetValue(objectPointer, out var info))
            {
                return info.TextureId;
            }
            
            return null;
        }
        */

        public object GetNativeImage(IntPtr objectPointer)
        {
            return this[objectPointer];
        }
    }
}
