using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using SkiaSharp;

namespace Drawie.Skia.Implementations
{
    public class SkiaShaderImplementation : SkObjectImplementation<SKShader>, IShaderImplementation
    {
        public SkiaImageImplementation ImageImplementation { get; set; }
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
            AddManagedInstance(skShader);
            return skShader.Handle;
        }

        public Shader? CreateFromString(string shaderCode, Uniforms uniforms, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(shaderCode, out errors);
            if (string.IsNullOrEmpty(errors))
            {
                var declaration = DeclarationsFromEffect(shaderCode, effect);
                SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, declaration, effect);
                SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);
                SKShader shader = effect.ToShader(effectUniforms, effectChildren);
                AddManagedInstance(shader);
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

                AddManagedInstance(shader);
                var declaration = DeclarationsFromEffect(shaderCode, effect);
                declarations[shader.Handle] = declaration;

#if DRAWIE_TRACE
            Trace(shader);
#endif
                return new Shader(shader.Handle, declaration);
            }

            return null;
        }

        public Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint((float)p1.X, (float)p1.Y),
                new SKPoint((float)p2.X, (float)p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                null,
                SKShaderTileMode.Clamp);

            if (shader == null) return null;

            AddManagedInstance(shader);
            return new Shader(shader.Handle);
        }

        public Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint((float)p1.X, (float)p1.Y),
                new SKPoint((float)p2.X, (float)p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                offsets,
                SKShaderTileMode.Clamp);

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif
            return new Shader(shader.Handle);
        }

        public Shader? CreateLinearGradient(VecD p1, VecD p2, Color[] colors, float[] offsets, Matrix3X3 localMatrix)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint((float)p1.X, (float)p1.Y),
                new SKPoint((float)p2.X, (float)p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                offsets,
                SKShaderTileMode.Clamp,
                localMatrix.ToSkMatrix());

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif
            return new Shader(shader.Handle);
        }

        public Shader? CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
            TileMode tileMode)
        {
            SKShader shader = SKShader.CreateRadialGradient(
                new SKPoint((float)center.X, (float)center.Y),
                radius,
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                colorPos,
                (SKShaderTileMode)tileMode);
            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif

            return new Shader(shader.Handle);
        }

        public Shader? CreateRadialGradient(VecD center, float radius, Color[] colors)
        {
            SKShader shader = SKShader.CreateRadialGradient(
                new SKPoint((float)center.X, (float)center.Y),
                radius,
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors), SKShaderTileMode.Clamp);

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif

            return new Shader(shader.Handle);
        }

        public Shader? CreateRadialGradient(VecD center, float radius, Color[] colors, float[] colorPos,
            Matrix3X3 localMatrix)
        {
            SKShader shader = SKShader.CreateRadialGradient(
                new SKPoint((float)center.X, (float)center.Y),
                radius,
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                colorPos,
                SKShaderTileMode.Clamp,
                localMatrix.ToSkMatrix());

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif

            return new Shader(shader.Handle);
        }

        public Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos, Matrix3X3 localMatrix)
        {
            SKShader shader = SKShader.CreateSweepGradient(
                new SKPoint((float)center.X, (float)center.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                colorPos,
                localMatrix.ToSkMatrix());

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif

            return new Shader(shader.Handle);
        }

        public Shader? CreateSweepGradient(VecD center, Color[] colors, float[] colorPos,
            TileMode tileMode, float angle,
            Matrix3X3 localMatrix)
        {
            SKShader shader = SKShader.CreateSweepGradient(
                new SKPoint((float)center.X, (float)center.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                colorPos,
                localMatrix.Concat(Matrix3X3.CreateRotationDegrees(angle - 90, (float)center.X, (float)center.Y))
                    .ToSkMatrix());

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif

            return new Shader(shader.Handle);
        }

        public Shader? CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves,
            float seed)
        {
            SKShader shader = SKShader.CreatePerlinNoiseTurbulence(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            if (shader == null) return null;

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif
            return new Shader(shader.Handle);
        }

        public Shader? CreatePerlinFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
        {
            if (baseFrequencyX <= 0 || baseFrequencyY <= 0)
                throw new ArgumentException("Base frequency must be greater than 0");

            SKShader shader = SKShader.CreatePerlinNoiseFractalNoise(
                baseFrequencyX,
                baseFrequencyY,
                numOctaves,
                seed);

            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif
            return new Shader(shader.Handle);
        }

        public object GetNativeShader(IntPtr objectPointer)
        {
            return this[objectPointer];
        }

        public Shader WithUpdatedUniforms(IntPtr objectPointer, Uniforms uniforms)
        {
            UnmanageAndDispose(objectPointer);

            if (!runtimeEffects.TryGetValue(objectPointer, out var effect))
            {
                throw new InvalidOperationException("Shader is not a runtime effect shader");
            }
            declarations.Remove(objectPointer, out var oldDeclarations);

            // TODO: Don't reupload shaders if they are the same
            SKRuntimeEffectUniforms effectUniforms = UniformsToSkUniforms(uniforms, oldDeclarations, effect);
            SKRuntimeEffectChildren effectChildren = UniformsToSkChildren(uniforms, effect);

            runtimeEffects.Remove(objectPointer);

            var newShader = effect.ToShader(effectUniforms, effectChildren);
            AddManagedInstance(newShader);

            runtimeEffects[newShader.Handle] = effect;
            declarations[newShader.Handle] = oldDeclarations;

#if DRAWIE_TRACE
            Trace(newShader);
#endif

            return new Shader(newShader.Handle, oldDeclarations);
        }

        public Shader SetLocalMatrix(IntPtr objectPointer, Matrix3X3 matrix)
        {
            if (!TryGetInstance(objectPointer, out var shader))
            {
                throw new InvalidOperationException("Shader does not exist");
            }

            return new Shader(shader.WithLocalMatrix(matrix.ToSkMatrix()).Handle);
        }

        public Shader? CreateBitmap(Bitmap bitmap, TileMode tileX, TileMode tileY, Matrix3X3 matrix)
        {
            SKBitmap skBitmap = bitmapImplementation[bitmap.ObjectPointer];
            SKShader shader = SKShader.CreateBitmap(skBitmap, (SKShaderTileMode)tileX, (SKShaderTileMode)tileY,
                matrix.ToSkMatrix());
            AddManagedInstance(shader);

#if DRAWIE_TRACE
            Trace(shader);
#endif
            return new Shader(shader.Handle);
        }

        public Shader? CreateCreate(Image image, TileMode tileX, TileMode tileY, Matrix3X3 matrix)
        {
            if (image == null)
            {
                return null;
            }

            SKImage target = ImageImplementation[image.ObjectPointer];
            SKShader shader = SKShader.CreateImage(target, (SKShaderTileMode)tileX, (SKShaderTileMode)tileY,
                matrix.ToSkMatrix());
            AddManagedInstance(shader);
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
            UnmanageAndDispose(shaderObjPointer);
            if (runtimeEffects.TryGetValue(shaderObjPointer, out var effect))
            {
                effect.Dispose();
                runtimeEffects.Remove(shaderObjPointer);
            }

            declarations.Remove(shaderObjPointer, out var declaration);
        }

        private SKRuntimeEffectUniforms UniformsToSkUniforms(Uniforms uniforms, List<UniformDeclaration> declarations,
            SKRuntimeEffect effect)
        {
            SKRuntimeEffectUniforms skUniforms = new SKRuntimeEffectUniforms(effect);
            foreach (var uniform in uniforms)
            {
                if (!skUniforms.Contains(uniform.Key))
                {
                    continue;
                }

                var declaration = declarations.FirstOrDefault(x => x.Name == uniform.Key);

                if (declaration.DataType == UniformValueType.Float)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatValue);
                }
                else if (declaration.DataType == UniformValueType.Color)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.ColorValue.ToSKColor());
                }
                else if (declaration.DataType == UniformValueType.Vector2)
                {
                    skUniforms.Add(uniform.Value.Name,
                        new SKPoint((float)uniform.Value.Vector2Value.X, (float)uniform.Value.Vector2Value.Y));
                }
                else if (declaration.DataType == UniformValueType.Vector3)
                {
                    skUniforms.Add(uniform.Value.Name,
                        new SKPoint3((float)uniform.Value.Vector3Value.X, (float)uniform.Value.Vector3Value.Y,
                            (float)uniform.Value.Vector3Value.Z));
                }
                else if (declaration.DataType == UniformValueType.Vector4)
                {
                    float[] values = new[]
                    {
                        (float)uniform.Value.Vector4Value.X, (float)uniform.Value.Vector4Value.Y,
                        (float)uniform.Value.Vector4Value.Z, (float)uniform.Value.Vector4Value.W
                    };

                    skUniforms.Add(uniform.Value.Name, values);
                }
                else if (declaration.DataType is UniformValueType.FloatArray or UniformValueType.Matrix3X3)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.FloatArrayValue);
                }
                else if (declaration.DataType == UniformValueType.Int)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.IntValue);
                }
                else if (declaration.DataType == UniformValueType.Vector2Int)
                {
                    skUniforms.Add(uniform.Value.Name,
                        new SKPointI((int)uniform.Value.Vector2IntValue.X, (int)uniform.Value.Vector2IntValue.Y));
                }
                else if (declaration.DataType is UniformValueType.Vector3Int or UniformValueType.Vector4Int or UniformValueType.IntArray)
                {
                    skUniforms.Add(uniform.Value.Name, uniform.Value.IntArrayValue);
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
                    skChildren.Add(uniform.Value.Name,
                        uniform.Value.ShaderValue == null ? null : this[uniform.Value.ShaderValue.ObjectPointer]);
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
                    if (codeChar == '/' && lastString.LastOrDefault() == '*')
                    {
                        isInBlockComment = false;
                        lastString = string.Empty;
                    }
                    else if (codeChar == '\n')
                    {
                        isInInlineComment = false;
                        lastString = string.Empty;
                    }

                    lastString += codeChar;

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

                    lastString += codeChar;
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

            if (nameLessBlock.Contains("int ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Int;
                return true;
            }

            if (nameLessBlock.Contains("float3x3 "))
            {
                detectedType = UniformValueType.Matrix3X3;
                return true;
            }

            if (nameLessBlock.Contains("float2 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec2 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half2 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector2;
                return true;
            }

            if (nameLessBlock.Contains("int2 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector2Int;
                return true;
            }

            if (nameLessBlock.Contains("float3 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec3 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half3 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector3;
                return true;
            }

            if (nameLessBlock.Contains("int3 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector3Int;
                return true;
            }

            if (nameLessBlock.Contains("float4 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("vec4 ", StringComparison.InvariantCultureIgnoreCase)
                || nameLessBlock.Contains("half4 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector4;
                return true;
            }

            if (nameLessBlock.Contains("int4 ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Vector4Int;
                return true;
            }

            if (nameLessBlock.Contains("shader ", StringComparison.InvariantCultureIgnoreCase))
            {
                detectedType = UniformValueType.Shader;
                return true;
            }

            detectedType = UniformValueType.FloatArray;
            return true;
        }
    }
}
