using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Numerics;

public class ColorSpaceTransformFn : NativeObject
{
    public ColorSpaceTransformFn(IntPtr objPtr) : base(objPtr)
    {
    }

    public override object Native =>
        DrawingBackendApi.Current.ColorSpaceImplementation.GetNativeNumericalTransformFunction(ObjectPointer);

    public float Transform(float x)
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.TransformNumerical(ObjectPointer, x);
    }
    
    public ColorSpaceTransformFn Invert()
    {
        return DrawingBackendApi.Current.ColorSpaceImplementation.InvertNumericalTransformFunction(ObjectPointer);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorSpaceImplementation.DisposeNumericalTransformFunction(ObjectPointer);
    }
}
