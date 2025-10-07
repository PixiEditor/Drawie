using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.Skia.Implementations;
using SkiaSharp;

namespace Drawie.Skia
{
    public static class ConversionExtensions
    {
        public static SKRect ToSKRect(this RectD rectD)
        {
            return SKRect.Create((float)rectD.X, (float)rectD.Y, (float)rectD.Width, (float)rectD.Height);
        }

        public static SKColor ToSKColor(this Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        public static SKMatrix ToSkMatrix(this Matrix3X3 matrix)
        {
            return new SKMatrix(matrix.ScaleX, matrix.SkewX, matrix.TransX, matrix.SkewY,
                matrix.ScaleY, matrix.TransY, matrix.Persp0, matrix.Persp1, matrix.Persp2);
        }

        public static Matrix3X3 ToMatrix3X3(this SKMatrix matrix)
        {
            return new Matrix3X3(matrix.ScaleX, matrix.SkewX, matrix.TransX, matrix.SkewY,
                matrix.ScaleY, matrix.TransY, matrix.Persp0, matrix.Persp1, matrix.Persp2);
        }

        public static SKPoint ToSkPoint(this VecF vector)
        {
            return new SKPoint(vector.X, vector.Y);
        }

        public static SKRect ToSkRect(this RectD rect)
        {
            return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }

        public static SKRect ToSkRect(this RectI rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static SKRectI ToSkRectI(this RectI rect)
        {
            return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static SKImageInfo ToSkImageInfo(this ImageInfo info)
        {
            SKColorSpace? colorSpace = null;

            if (info.ColorSpace != null)
            {
                colorSpace =
                    (SKColorSpace)DrawingBackendApi.Current.ColorSpaceImplementation.GetNativeColorSpace(info.ColorSpace
                        .ObjectPointer);
            }

            return new SKImageInfo(info.Width, info.Height, (SKColorType)info.ColorType, (SKAlphaType)info.AlphaType,
                colorSpace);
        }

        public static Color ToBackendColor(this SKColor color)
        {
            return new Color(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static ColorF ToBackendColorF(this SKColorF color)
        {
            return new ColorF(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static ImageInfo ToImageInfo(this SKImageInfo info)
        {
            ColorSpace? cs = null;
            if (info.ColorSpace != null)
            {
                cs = new ColorSpace(info.ColorSpace.Handle);
                var colorSpaceImpl = DrawingBackendApi.Current.ColorSpaceImplementation as SkiaColorSpaceImplementation;
                colorSpaceImpl.AddManagedInstance(info.ColorSpace);
            }

            return new ImageInfo(info.Width, info.Height,
                (ColorType)info.ColorType,
                (AlphaType)info.AlphaType,
                cs);
        }
    }
}
