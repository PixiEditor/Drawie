using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaPathEffectImplementation : SkObjectImplementation<SKPathEffect>, IPathEffectImplementation
{
    public IntPtr CreateDash(float[] intervals, float phase)
    {
        SKPathEffect skPathEffect = SKPathEffect.CreateDash(intervals, phase);
        ManagedInstances[skPathEffect.Handle] = skPathEffect;
        return skPathEffect.Handle;
    }

    public void Dispose(IntPtr pathEffectPointer)
    {
        if (!ManagedInstances.TryGetValue(pathEffectPointer, out var pathEffect)) return;

        pathEffect.Dispose();
        ManagedInstances.TryRemove(pathEffectPointer, out _);
    }

    public object GetNativePathEffect(IntPtr objectPointer)
    {
        if (!ManagedInstances.TryGetValue(objectPointer, out var pathEffect))
        {
            return null;
        }

        return pathEffect;
    }
}
