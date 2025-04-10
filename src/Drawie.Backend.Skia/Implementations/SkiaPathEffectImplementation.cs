using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaPathEffectImplementation : SkObjectImplementation<SKPathEffect>, IPathEffectImplementation
{
    public IntPtr CreateDash(float[] intervals, float phase)
    {
        SKPathEffect skPathEffect = SKPathEffect.CreateDash(intervals, phase);
        AddManagedInstance(skPathEffect);
        return skPathEffect.Handle;
    }

    public void Dispose(IntPtr pathEffectPointer)
    {
        UnmanageAndDispose(pathEffectPointer);
    }

    public object? GetNativePathEffect(IntPtr objectPointer)
    {
        return GetInstanceOrDefault(objectPointer);
    }
}
