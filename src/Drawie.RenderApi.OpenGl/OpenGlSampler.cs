using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlSampler : ISampler
{
    public uint Handle { get; }

    public OpenGlSampler(GL api)
    {
        Handle = api.CreateSampler();
        api.SamplerParameter(
            Handle,
            SamplerParameterI.MinFilter,
            (int)TextureMinFilter.Linear);

        api.SamplerParameter(
            Handle,
            SamplerParameterI.MagFilter,
            (int)TextureMagFilter.Linear);

        api.SamplerParameter(
            Handle,
            SamplerParameterI.WrapS,
            (int)TextureWrapMode.Repeat);

        api.SamplerParameter(
            Handle,
            SamplerParameterI.WrapT,
            (int)TextureWrapMode.Repeat);
    }
}