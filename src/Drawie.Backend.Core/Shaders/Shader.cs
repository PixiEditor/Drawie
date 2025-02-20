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

    public IReadOnlyList<UniformDeclaration> UniformDeclarations { get; }

    public Shader(IntPtr objPtr, List<UniformDeclaration> declarations = null) : base(objPtr)
    {
        UniformDeclarations = declarations ?? new List<UniformDeclaration>();
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

    /// <summary>
    ///    Creates shader from string. If shader has errors, exception is thrown.
    /// </summary>
    /// <param name="shaderCode">Code of shader</param>
    /// <param name="uniforms">Uniforms for shader</param>
    /// <returns>Created shader</returns>
    /// <exception cref="ShaderCompilationException">If shader has errors.</exception>
    public static Shader Create(string shaderCode, Uniforms uniforms)
    {
        var created =
            DrawingBackendApi.Current.ShaderImplementation.CreateFromString(shaderCode, uniforms, out string errors);

        if (!string.IsNullOrEmpty(errors))
        {
            throw new ShaderCompilationException(errors, shaderCode);
        }

        return created!;
    }

    public static Shader? Create(string shaderCode, out string errors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateFromString(shaderCode, out errors);
    }

    public static Shader? Create(string shaderCode, Uniforms uniforms, out string errors)
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

    public static UniformDeclaration[]? GetUniformDeclarations(string shaderCode)
    {
        return DrawingBackendApi.Current.ShaderImplementation.GetUniformDeclarations(shaderCode);
    }
}
