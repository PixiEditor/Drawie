using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;

namespace Drawie.Backend.Core.Surfaces.ImageData;

public class ColorSpace : NativeObject
{
    public override object Native =>
        DrawingBackendApi.Current.ColorSpaceImplementation.GetNativeColorSpace(ObjectPointer);

    public bool IsSrgb => DrawingBackendApi.Current.ColorSpaceImplementation.IsSrgb(ObjectPointer);

    public ColorSpace(IntPtr objPtr) : base(objPtr)
    {
    }

    public static ColorSpace CreateSrgb()
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.CreateSrgb();
    }

    public static ColorSpace CreateSrgbLinear()
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.CreateSrgbLinear();
    }


    public ColorSpaceTransformFn GetTransformFunction()
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.GetTransformFunction(ObjectPointer);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorSpaceImplementation.Dispose(ObjectPointer);
    }

    public override bool Equals(object? obj)
    {
        if(obj is ColorSpace other)
        {
            return ObjectPointer == other.ObjectPointer || this.IsSrgb && other.IsSrgb || this == other;
        }

        return false;
    }
}
