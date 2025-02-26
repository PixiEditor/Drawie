using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IColorSpaceImplementation
{
    public ColorSpace CreateSrgb();
    public ColorSpace CreateSrgbLinear();
    public void Dispose(IntPtr objectPointer);
    public object GetNativeColorSpace(IntPtr objectPointer);
    public bool IsSrgb(IntPtr objectPointer);
    public object GetNativeNumericalTransformFunction(IntPtr objectPointer);
    public void DisposeNumericalTransformFunction(IntPtr objectPointer);
    public ColorSpaceTransformFn GetTransformFunction(IntPtr colorSpacePointer);
    public float TransformNumerical(IntPtr objectPointer, float value);
    public ColorSpaceTransformFn InvertNumericalTransformFunction(IntPtr objectPointer);
    public float[] GetTransformFunctionValues(IntPtr objectPointer);
}
