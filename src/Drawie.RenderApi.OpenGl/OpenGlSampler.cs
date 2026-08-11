using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlSampler : ISampler
{
    public uint SamplerId { get; }

    public OpenGlSampler(GL api)
    {
        SamplerId = api.CreateSampler();
        api.SamplerParameter(
            SamplerId,
            SamplerParameterI.MinFilter,
            (int)TextureMinFilter.Linear);

        api.SamplerParameter(
            SamplerId,
            SamplerParameterI.MagFilter,
            (int)TextureMagFilter.Linear);

        api.SamplerParameter(
            SamplerId,
            SamplerParameterI.WrapS,
            (int)TextureWrapMode.Repeat);

        api.SamplerParameter(
            SamplerId,
            SamplerParameterI.WrapT,
            (int)TextureWrapMode.Repeat);
    }
}