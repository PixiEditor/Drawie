using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Shaders;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaBlenderImplementation : SkObjectImplementation<SKBlender>, IBlenderImplementation
{
    private SkiaShaderImplementation shaderImpl;

    public SkiaBlenderImplementation(SkiaShaderImplementation shaderImpl)
    {
        this.shaderImpl = shaderImpl;
    }

    public IntPtr CreateFromString(string blenderCode, out string? errors)
    {
        using var effect = SKRuntimeEffect.CreateBlender(blenderCode, out errors);
        if (!string.IsNullOrEmpty(errors) || effect == null)
        {
            return IntPtr.Zero;
        }

        var blender = effect.ToBlender();
        if (blender == null)
        {
            return IntPtr.Zero;
        }

        return AddManagedInstance(blender);
    }

    public IntPtr CreateFromString(string blenderCode, Uniforms uniforms, out string? errors)
    {
        using var effect = SKRuntimeEffect.CreateBlender(blenderCode, out errors);
        if (!string.IsNullOrEmpty(errors) || effect == null)
        {
            return IntPtr.Zero;
        }
        var declaration = SkiaShaderImplementation.DeclarationsFromEffect(blenderCode, effect);
        SKRuntimeEffectUniforms effectUniforms = SkiaShaderImplementation.UniformsToSkUniforms(uniforms, declaration, effect);
        SKRuntimeEffectChildren effectChildren = SkiaShaderImplementation.UniformsToSkChildren(uniforms, effect, shaderImpl);
        var blender = effect.ToBlender(effectUniforms, effectChildren);
        return AddManagedInstance(blender);
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
