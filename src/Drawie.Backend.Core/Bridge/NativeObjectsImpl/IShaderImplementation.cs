using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IShaderImplementation
{
    public string ShaderLanguageExtension { get; }
    public IntPtr CreateShader();
    public void Dispose(IntPtr shaderObjPointer);
    public Shader? CreateFromString(string shaderCode, out string errors);
    public Shader? CreateFromString(string shaderCode, Uniforms uniforms, out string errors);
    public Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors);
    public Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets);
    public Shader CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets, Matrix3X3 localMatrix);

    public Shader CreateRadialGradient(VecD center, float radius, Color[] colors);

    public Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        ShaderTileMode tileMode);

    public Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
    public Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
    public object GetNativeShader(IntPtr objectPointer);
    public Shader WithUpdatedUniforms(IntPtr objectPointer, Uniforms uniforms);
    public void SetLocalMatrix(IntPtr objectPointer, Matrix3X3 matrix);
    public Shader? CreateBitmap(Bitmap bitmap, ShaderTileMode tileX, ShaderTileMode tileY, Matrix3X3 matrix);
    public UniformDeclaration[] GetUniformDeclarations(string shaderCode);
    public Shader? CreateCreate(Image image, ShaderTileMode tileX, ShaderTileMode tileY, Matrix3X3 matrix);

    public Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        Matrix3X3 localMatrix);

    public Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos, Matrix3X3 localMatrix);

    public Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos,
        ShaderTileMode tileMode, float angle, Matrix3X3 localMatrix);
}
