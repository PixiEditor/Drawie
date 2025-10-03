﻿using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces;

public class Pixmap : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PixmapImplementation.GetNativePixmap(ObjectPointer);

    internal Pixmap(IntPtr objPtr) : base(objPtr)
    {
    }

    public static Pixmap InternalCreateFromExistingPointer(IntPtr objPointer)
    {
        return new Pixmap(objPointer);
    }
    
    public Pixmap(ImageInfo imgInfo, IntPtr dataPtr) : base(dataPtr)
    {
        ObjectPointer = DrawingBackendApi.Current.PixmapImplementation.Construct(dataPtr, imgInfo);
    }

    public int Width
    {
        get => DrawingBackendApi.Current.PixmapImplementation.GetWidth(this);
    }

    public int Height
    {
        get => DrawingBackendApi.Current.PixmapImplementation.GetHeight(this);
    }

    public int BytesSize => DrawingBackendApi.Current.PixmapImplementation.GetBytesSize(this);

    public override void Dispose()
    {
        DrawingBackendApi.Current.PixmapImplementation.Dispose(ObjectPointer);
    }

    public Color GetPixelColor(int x, int y) => GetPixelColor(new VecI(Math.Clamp(x, 0, Width), Math.Clamp(y, 0, Height)));
    
    public Color GetPixelColor(VecI position)
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixelColor(ObjectPointer, position);
    }

    public IntPtr GetPixels()
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixels(ObjectPointer);
    }

    public Span<T> GetPixelSpan<T>() where T : unmanaged
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixelSpan<T>(this);
    }

    public ColorF GetPixelColorPrecise(VecI pos)
    {
        return DrawingBackendApi.Current.PixmapImplementation.GetPixelColorF(ObjectPointer, pos);
    }
}
