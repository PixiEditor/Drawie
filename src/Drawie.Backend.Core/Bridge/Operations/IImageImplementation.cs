﻿using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.Operations
{
    public interface IImageImplementation
    {
        public Image Snapshot(DrawingSurface drawingSurface);
        public Image Snapshot(DrawingSurface drawingSurface, RectI bounds);
        public void DisposeImage(Image image);
        public Image? FromEncodedData(string path);
        public Image? FromEncodedData(byte[] dataBytes);
        public Image? FromPixelCopy(ImageInfo info, byte[] pixels);
        public void GetColorShifts(ref int platformColorAlphaShift, ref int platformColorRedShift, ref int platformColorGreenShift, ref int platformColorBlueShift);
        public ImgData Encode(Image image);
        public ImgData Encode(Image image, EncodedImageFormat format, int quality);
        public int GetWidth(IntPtr objectPointer);
        public int GetHeight(IntPtr objectPointer);
        public object GetNativeImage(IntPtr objectPointer);
        public Image Clone(Image image);
        public Pixmap PeekPixels(IntPtr objectPointer);
        public ImageInfo GetImageInfo(IntPtr objectPointer);
        public Shader ToShader(IntPtr objectPointer);
        public Shader ToRawShader(IntPtr objectPointer);
        public Shader? ToShader(IntPtr objectPointer, TileMode clamp, TileMode tileMode, Matrix3X3 fillMatrixValue);
    }
}
