using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.WebGl;

public class WebGlSampler : ISampler
{
    public int Gl { get; }
    public uint Handle { get; }

    public WebGlSampler(int glHandle, SamplerDesc desc)
    {
        Gl = glHandle;
        Handle = (uint)JSRuntime.CreateSampler(glHandle);
    }
}