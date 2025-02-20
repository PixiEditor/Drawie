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
                ManagedInstances[shader.Handle] = shader;
                runtimeEffects[shader.Handle] = effect;
                return new Shader(shader.Handle);
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
                return new Shader(shader.Handle);
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

            var newShader = effect.ToShader(effectUniforms, effectChildren);
            ManagedInstances[newShader.Handle] = newShader;
            runtimeEffects[newShader.Handle] = effect;

            return new Shader(newShader.Handle);
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

        public void Dispose(IntPtr shaderObjPointer)
        {
            if (!ManagedInstances.TryGetValue(shaderObjPointer, out var shader)) return;
            shader.Dispose();
            ManagedInstances.TryRemove(shaderObjPointer, out _);
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
    }
}
