using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaBlenderImplementation : SkObjectImplementation<SKBlender>, IBlenderImplementation
{
    public IntPtr CreateFromString(string blenderCode, out string? errors)
    {
        using var effect = SKRuntimeEffect.CreateBlender(blenderCode, out errors);
        if (!string.IsNullOrEmpty(errors) || effect == null)
        {
            return IntPtr.Zero;
        }

        var blender = effect.ToBlender();
        AddManagedInstance(blender);

        return blender.Handle;
    }

    public object GetNativeObject(IntPtr objectPointer)
    {
        return GetInstanceOrDefault(objectPointer);
    }

    public void Dispose(IntPtr objectPointer)
    {
        UnmanageAndDispose(objectPointer);
    }
}
