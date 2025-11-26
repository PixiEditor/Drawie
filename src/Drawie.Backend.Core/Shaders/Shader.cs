using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Exceptions;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders;

public class Shader : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ShaderImplementation.GetNativeShader(ObjectPointer);

    public IReadOnlyList<UniformDeclaration> UniformDeclarations { get; }

    public HashSet<object> LockedDisposeTargets { get; } = new();

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
        if (LockedDisposeTargets.Count > 0)
        {
            return;
        }

        DrawingBackendApi.Current.ShaderImplementation.Dispose(ObjectPointer);
    }

    public static Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors);
    }


    public static Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets, Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors, offsets,
            localMatrix);
    }

    public static Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors, offsets);
    }

    public static Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves,
        float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinNoiseTurbulence(baseFrequencyX,
            baseFrequencyY, numOctaves, seed);
    }

    public static Shader CreateRadialGradient(VecD center, float radius, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors);
    }

    public static Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors, colorPos,
            localMatrix);
    }

    public static Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        TileMode tileMode)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors, colorPos,
            tileMode);
    }


    public static Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos, Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateSweepGradient(center, colors, colorPos,
            localMatrix);
    }

    public static Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos,
        TileMode tileMode, float angle,
        Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateSweepGradient(center, colors, colorPos, tileMode,
            angle, localMatrix);
    }

    public static Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves,
        float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinFractalNoise(baseFrequencyX, baseFrequencyY,
            numOctaves, seed);
    }

    public Shader SetLocalMatrix(Matrix3X3 matrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.SetLocalMatrix(ObjectPointer, matrix);
    }

    public static Shader? CreateBitmap(Bitmap bitmap, TileMode tileX, TileMode tileY, Matrix3X3 matrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateBitmap(bitmap, tileX, tileY, matrix);
    }

    public static Shader? CreateImage(Image image, TileMode tileX, TileMode tileY, Matrix3X3 matrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateCreate(image, tileX, tileY, matrix);
    }

    public static UniformDeclaration[]? GetUniformDeclarations(string shaderCode)
    {
        return DrawingBackendApi.Current.ShaderImplementation.GetUniformDeclarations(shaderCode);
    }

    public void LockDispose(object locker)
    {
        LockedDisposeTargets.Add(locker);
    }

    public void UnlockDispose(object locker)
    {
        LockedDisposeTargets.Remove(locker);
    }
}
