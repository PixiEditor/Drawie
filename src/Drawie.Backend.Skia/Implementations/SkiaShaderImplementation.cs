using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaShaderImplementation : SkObjectImplementation<SKShader>, IShaderImplementation
    {
        private SkiaBitmapImplementation bitmapImplementation;
        private Dictionary<IntPtr, SKRuntimeEffect> runtimeEffects = new();
        private Dictionary<IntPtr, List<UniformDeclaration>> declarations = new();

        public SkiaShaderImplementation()
        {
        }

        public void SetBitmapImplementation(SkiaBitmapImplementation bitmapImplementation)
        {
            this.bitmapImplementation = bitmapImplementation;
        }

        public string ShaderLanguageExtension { get; } = "sksl";

        public IntPtr CreateShader()
        {
            SKShader skShader = SKShader.CreateEmpty();
            ManagedInstances[skShader.Handle] = skShader;
            return skShader.Handle;
        }

        public Shader? CreateFromString(string shaderCode, Uniforms uniforms, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(shaderCode, out errors);
            if (string.IsNullOrEmpty(errors))
            {
                SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, effect);
                SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);
                SKShader shader = effect.ToShader(effectUniforms, effectChildren);
                var declaration = DeclarationsFromEffect(shaderCode, effect);
                ManagedInstances[shader.Handle] = shader;
                runtimeEffects[shader.Handle] = effect;
                declarations[shader.Handle] = declaration;
                return new Shader(shader.Handle, declaration);
            }

            return null;
        }

        public Shader? CreateFromString(string shaderCode, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(shaderCode, out errors);
            if (string.IsNullOrEmpty(errors))
            {
                SKShader shader = effect.ToShader();
                if (shader == null)
                {
                    return null;
                }

                ManagedInstances[shader.Handle] = shader;
                var declaration = DeclarationsFromEffect(shaderCode, effect);
                declarations[shader.Handle] = declaration;

                return new Shader(shader.Handle, declaration);
            }

            return null;
        }

        public Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint(p1.X, p1.Y),
                new SKPoint(p2.X, p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                null,
                SKShaderTileMode.Clamp);
            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public Shader CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
            ShaderTileMode tileMode)
        {
            SKShader shader = SKShader.CreateRadialGradient(
                new SKPoint((float)center.X, (float)center.Y),
                radius,
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                colorPos,
                (SKShaderTileMode)tileMode);
            ManagedInstances[shader.Handle] = shader;

            return new Shader(shader.Handle);
        }

        public Shader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves,
            float seed)
        {
            SKShader shader = SKShader.CreatePerlinNoiseTurbulence(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public Shader CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
        {
            if (baseFrequencyX <= 0 || baseFrequencyY <= 0)
                throw new ArgumentException("Base frequency must be greater than 0");

            SKShader shader = SKShader.CreatePerlinNoiseFractalNoise(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public object GetNativeShader(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }

        public Shader WithUpdatedUniforms(IntPtr objectPointer, Uniforms uniforms)
        {
            if (!ManagedInstances.TryGetValue(objectPointer, out var shader))
            {
                throw new InvalidOperationException("Shader does not exist");
            }

            if (!runtimeEffects.TryGetValue(objectPointer, out var effect))
            {
                throw new InvalidOperationException("Shader is not a runtime effect shader");
            }

            // TODO: Don't reupload shaders if they are the same
            SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, effect);
            SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);

            shader.Dispose();
            ManagedInstances.TryRemove(objectPointer, out _);
            runtimeEffects.Remove(objectPointer);
            declarations.Remove(objectPointer, out var oldDeclarations);

            var newShader = effect.ToShader(effectUniforms, effectChildren);
            ManagedInstances[newShader.Handle] = newShader;
            runtimeEffects[newShader.Handle] = effect;
            declarations[newShader.Handle] = oldDeclarations;

            return new Shader(newShader.Handle, oldDeclarations);
        }

        public void SetLocalMatrix(IntPtr objectPointer, Matrix3X3 matrix)
        {
            if (!ManagedInstances.TryGetValue(objectPointer, out var shader))
            {
                throw new InvalidOperationException("Shader does not exist");
            }

            shader.WithLocalMatrix(matrix.ToSkMatrix());
        }

        public Shader? CreateBitmap(Bitmap bitmap, ShaderTileMode tileX, ShaderTileMode tileY, Matrix3X3 matrix)
        {
            SKBitmap skBitmap = bitmapImplementation.ManagedInstances[bitmap.ObjectPointer];
            SKShader shader = SKShader.CreateBitmap(skBitmap, (SKShaderTileMode)tileX, (SKShaderTileMode)tileY,
                matrix.ToSkMatrix());
            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public UniformDeclaration[]? GetUniformDeclarations(string shaderCode)
        {
            using SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(shaderCode, out string errors);
            if (!string.IsNullOrEmpty(errors) || effect == null)
            {
                return null;
            }

            return DeclarationsFromEffect(shaderCode, effect).ToArray();
        }

        public void Dispose(IntPtr shaderObjPointer)
        {
            ManagedInstances.TryRemove(shaderObjPointer, out var shader);
            if (shader == null)
            {
                return;
            }

            shader.Dispose();
            if (runtimeEffects.TryGetValue(shaderObjPointer, out var effect))
            {
                effect.Dispose();
                runtimeEffects.Remove(shaderObjPointer);
            }

            declarations.Remove(shaderObjPointer, out var declaration);
        }

        private SKRuntimeEffectUniforms UniformsToSkUniforms(Uniforms uniforms, SKRuntimeEffect effect)
        {
            SKRuntimeEffectUniforms skUniforms = new SKRuntimeEffectUniforms(effect);
            foreach (var uniform in uniforms)
            {
                if (!skUniforms.Contains(uniform.Key))
                {
                    continue;
                }

                if (uniform.Value.DataType == UniformValueType.Float)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatValue);
                }
                else if (uniform.Value.DataType == UniformValueType.Color)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.ColorValue.ToSKColor());
                }
                else if (uniform.Value.DataType == UniformValueType.Vector2)
                {
                    skUniforms.Add(uniform.Value.Name,
                        new SKPoint((float)uniform.Value.Vector2Value.X, (float)uniform.Value.Vector2Value.Y));
                }
                else if (uniform.Value.DataType == UniformValueType.Vector3)
                {
                    skUniforms.Add(uniform.Value.Name,
                        new SKPoint3((float)uniform.Value.Vector3Value.X, (float)uniform.Value.Vector3Value.Y,
                            (float)uniform.Value.Vector3Value.Z));
                }
                else if (uniform.Value.DataType == UniformValueType.Vector4)
                {
                    float[] values = new[]
                    {
                        (float)uniform.Value.Vector4Value.X, (float)uniform.Value.Vector4Value.Y,
                        (float)uniform.Value.Vector4Value.Z, (float)uniform.Value.Vector4Value.W
                    };

                    skUniforms.Add(uniform.Value.Name, values);
                }
                else if (uniform.Value.DataType == UniformValueType.FloatArray)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatArrayValue);
                }
            }

            return skUniforms;
        }

        private SKRuntimeEffectChildren UniformsToSkChildren(Uniforms uniforms, SKRuntimeEffect effect)
        {
            SKRuntimeEffectChildren skChildren = new SKRuntimeEffectChildren(effect);
            foreach (var uniform in uniforms)
            {
                if (!skChildren.Contains(uniform.Key))
                {
                    continue;
                }

                if (uniform.Value.DataType == UniformValueType.Shader)
                {
                    skChildren.Add(uniform.Value.Name, this[uniform.Value.ShaderValue.ObjectPointer]);
                }
            }

            return skChildren;
        }

        private static List<UniformDeclaration> DeclarationsFromEffect(string code, SKRuntimeEffect effect)
        {
            List<UniformDeclaration> declarations = new();
            foreach (var uniform in effect.Uniforms)
            {
                if (uniform == null) continue;
                UniformValueType? detectedType = FindUniformType(code, uniform);
                if (detectedType == null)
                {
                    continue;
                }

                declarations.Add(new UniformDeclaration(uniform, detectedType.Value));
            }

            foreach (var child in effect.Children)
            {
                if (child == null) continue;
                declarations.Add(new UniformDeclaration(child, UniformValueType.Shader));
            }

            return declarations;
        }

        public static UniformValueType? FindUniformType(string code, string uniform)
        {
            string uniformName = uniform;

            string lastString = string.Empty;
            bool isInInlineComment = false;
            bool isInBlockComment = false;

            foreach (var codeChar in code)
            {
                if (isInBlockComment || isInInlineComment)
                {
                    if (codeChar == '*' && lastString[^1] == '/')
                    {
                        isInBlockComment = false;
                        lastString = string.Empty;
                    }
                    else if (codeChar == '\n')
                    {
                        isInInlineComment = false;
                        lastString = string.Empty;
                    }

                    continue;
                }

                if (codeChar == ';')
                {
                    if (lastString.Contains(uniformName) &&
                        TryDetectType(lastString, uniformName, out var detectedType))
                    {
                        return detectedType.Value;
                    }

                    lastString = string.Empty;
                }
                else if (codeChar == '/')
                {
                    if (lastString.LastOrDefault() == '/')
                    {
                        isInInlineComment = true;
                        lastString = string.Empty;
                    }
                }
                else if (codeChar == '*' && lastString.LastOrDefault() == '/')
                {
                    isInBlockComment = true;
                    lastString = string.Empty;
                }
                else if (!isInInlineComment && !isInBlockComment && codeChar != '\n' && codeChar != '\r' &&
                         codeChar != '\t')
                {
                    lastString += codeChar;
                }
            }

            return null;
        }

        private static bool TryDetectType(string lastString, string name, out UniformValueType? detectedType)
        {
            if (!lastString.Contains("uniform ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = null;
                return false;
            }

            string nameLessBlock = lastString.Replace(name, string.Empty);

            if (nameLessBlock.Contains("color", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Color;
                return true;
            }

            if (nameLessBlock.Contains("float ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Float;
                return true;
            }

            if (nameLessBlock.Contains("float2", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec2", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half2", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector2;
                return true;
            }

            if (nameLessBlock.Contains("float3", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec3", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half3", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector3;
                return true;
            }

            if (nameLessBlock.Contains("float4", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec4", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half4", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector4;
                return true;
            }

            if (nameLessBlock.Contains("shader", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Shader;
                return true;
            }

            detectedType = UniformValueType.FloatArray;
            return true;
        }
    }
}
