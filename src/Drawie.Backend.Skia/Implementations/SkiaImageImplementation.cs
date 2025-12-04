using Drawie.Backend.Core.Bridge.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.Skia.Encoders;
using Drawie.Skia.Extensions;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaImageImplementation : SkObjectImplementation<SKImage>, IImageImplementation
    {
        private readonly SkObjectImplementation<SKData> _imgImplementation;
        private readonly SkiaPixmapImplementation _pixmapImplementation;
        private SkObjectImplementation<SKSurface>? _surfaceImplementation;
        private SkiaShaderImplementation shaderImpl;

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

        public void SetSurfaceImplementation(SkObjectImplementation<SKSurface> surfaceImplementation)
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

        public object GetNativeImage(IntPtr objectPointer)
        {
            return this[objectPointer];
        }
    }
}
