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

    public static Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors);
    }


    public static Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets, Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors, offsets,
            localMatrix);
    }

    public static Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateLinearGradient(p1, p2, colors, offsets);
    }

    public static Shader? CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves,
        float seed)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreatePerlinNoiseTurbulence(baseFrequencyX,
            baseFrequencyY, numOctaves, seed);
    }

    public static Shader? CreateVoronoi(float frequency, int octaves, float seed, int feature, float randomness, float angleOffset)
    {
        string voronoiShaderCode = """
                                   uniform float iSeed;
                                   uniform float iFrequency;
                                   uniform int iOctaves;
                                   uniform float iRandomness;
                                   uniform int iFeature;
                                   uniform float iAngleOffset;

                                   const int MAX_OCTAVES = 8;
                                   const float LARGE_NUMBER = 1e9;
                                   const float FEATURE_SEED_SCALE = 10.0;
                                   const float PI = 3.14159265;

                                   float hashPoint(float2 p, float seed) {
                                       p = fract(p * float2(0.3183099, 0.3678794) + seed);
                                       p += dot(p, p.yx + 19.19);
                                       return fract(p.x * p.y);
                                   }

                                   float2 getFeaturePoint(float2 cell, float seed, float randomness, float angleOffset) {
                                       float2 randomCellOffset = float2(
                                           hashPoint(cell, seed),
                                           hashPoint(cell, seed + 17.0)
                                       );
                                       
                                       float2 featurePoint = mix(float2(0.5, 0.5), randomCellOffset, randomness);
                                       
                                       float angle = hashPoint(cell, seed + 53.0) * PI * 2.0;
                                       angle += angleOffset;
                                       
                                       float2 dir = float2(cos(angle), sin(angle));
                                       float offsetAmount = 0.15;
                                       featurePoint += dir * offsetAmount * randomness;
                                       
                                       featurePoint = clamp(featurePoint, 0.0, 1.0);
                                       
                                       return featurePoint;
                                   }

                                   float2 getVoronoiDistances(float2 pos, float seed) {
                                       float2 cell = floor(pos);
                                       float minDist = LARGE_NUMBER;
                                       float secondMinDist = LARGE_NUMBER;

                                       for (int y = -1; y <= 1; y++) {
                                           for (int x = -1; x <= 1; x++) {
                                               float2 neighborCell = cell + float2(float(x), float(y));
                                               float2 featurePoint = getFeaturePoint(neighborCell, seed, iRandomness, iAngleOffset);
                                               float2 delta = pos - (neighborCell + featurePoint);
                                               float dist = length(delta);
                                               
                                               if (dist < minDist) {
                                                   secondMinDist = minDist;
                                                   minDist = dist;
                                               } else if (dist < secondMinDist) {
                                                   secondMinDist = dist;
                                               }
                                           }
                                       }
                                       return float2(minDist, secondMinDist);
                                   }

                                   half4 main(float2 uv) {
                                       float noiseSum = 0.0;
                                       float amplitude = 1.0;
                                       float amplitudeSum = 0.0;

                                       for (int octave = 0; octave < MAX_OCTAVES; octave++) {
                                           if (octave >= iOctaves) break;

                                           float freq = iFrequency * exp2(float(octave));
                                           float2 samplePos = uv * freq;
                                           
                                           float dist = 0.0;
                                           float2 distances = getVoronoiDistances(samplePos, iSeed + float(octave) * FEATURE_SEED_SCALE);
                                           float f1 = distances.x;
                                           float f2 = distances.y;
                                           
                                           if (iFeature == 0) {
                                               dist = f1;
                                           }
                                           else if (iFeature == 1) {
                                               dist = f2;
                                           }
                                           else if (iFeature == 2) {
                                               dist = f2 - f1;
                                           }

                                           noiseSum += dist * amplitude;
                                           amplitudeSum += amplitude;
                                           amplitude *= 0.5;
                                       }

                                       return half4(noiseSum / amplitudeSum);
                                   }
                                   """;

        Uniforms uniforms = new Uniforms();
        uniforms.Add("iSeed", new Uniform("iSeed", seed));
        uniforms.Add("iFrequency", new Uniform("iFrequency", frequency));
        uniforms.Add("iOctaves", new Uniform("iOctaves", octaves));
        uniforms.Add("iRandomness", new Uniform("iRandomness", randomness));
        uniforms.Add("iFeature", new Uniform("iFeature", feature));
        uniforms.Add("iAngleOffset", new Uniform("iAngleOffset", angleOffset));

        return Shader.Create(voronoiShaderCode, uniforms, out _);
    }

    public static Shader? CreateRadialGradient(VecD center, float radius, Color[] colors)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors);
    }

    public static Shader? CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
        Matrix3X3 localMatrix)
    {
        return DrawingBackendApi.Current.ShaderImplementation.CreateRadialGradient(center, radius, colors, colorPos,
            localMatrix);
    }

    public static Shader? CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
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

    public static Shader? CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves,
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
