using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Exceptions;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders;

public class Shader : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ShaderImplementation.GetNativeShader(ObjectPointer);

    public Shader(IntPtr objPtr) : base(objPtr)
    {
    }

    public Shader(string shaderCode, Uniforms uniforms) : base(DrawingBackendApi.Current.ShaderImplementation
        .CreateFromString(shaderCode, uniforms, out string errors)?.ObjectPointer ?? IntPtr.Zero)
    {
        if (!string.IsNullOrEmpty(errors))
        {
            throw new ShaderCompilationException(errors, shaderCode);
        }
    }

    /// <summary>
    ///     Creates updated version of shader with new uniforms. THIS FUNCTION DISPOSES OLD SHADER.
    /// </summary>
    /// <param name="uniforms"></param>
    /// <returns></returns>
    public Shader WithUpdatedUniforms(Uniforms uniforms)
    {
        return DrawingBackendApi.Current.ShaderImplementation.WithUpdatedUniforms(ObjectPointer, uniforms);
    }

    public static Shader? CreateFromString(string shaderCode, out string errors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateFromString(shaderCode, out errors);
    }

    public static Shader? CreateFromString(string shaderCode, Uniforms uniforms, out string errors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateFromString(shaderCode, uniforms, out errors);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ShaderImplementation.Dispose(ObjectPointer);
    }

    public static Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors);
    }

    public static Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves,
        float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinNoiseTurbulence(baseFrequencyX,
            baseFrequencyY, numOctaves, seed);
    }

    public static Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        ShaderTileMode tileMode)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors, colorPos,
            tileMode);
    }

    public static Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves,
        float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinFractalNoise(baseFrequencyX, baseFrequencyY,
            numOctaves, seed);
    }

    public void SetLocalMatrix(Matrix3X3 matrix)
    {
        DrawingBackendApi.Current.ShaderImplementation.SetLocalMatrix(ObjectPointer, matrix);
    }

    public static Shader? CreateBitmap(Bitmap bitmap, ShaderTileMode tileX, ShaderTileMode tileY, Matrix3X3 matrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateBitmap(bitmap, tileX, tileY, matrix);
    }
}
