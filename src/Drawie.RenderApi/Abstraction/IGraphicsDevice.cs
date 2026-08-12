using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.Abstraction;

public interface IGraphicsDevice
{
    IBuffer<TData> CreateBuffer<TData>(BufferUsage usage, TData[]? data) where TData : unmanaged;
    ITexture CreateTexture(TextureDesc desc);
    IPipeline CreatePipeline(PipelineDesc desc);
    ICommandList CreateCommandList();

    Dictionary<int, INativeObject> ManagedObjects { get; }
    ISampler CreateSampler(SamplerDesc desc);

    void Submit(RecordedRenderPass cmdList);
    IShaderProgram CreateShaderProgram(ShaderProgramDesc shaderProgramDesc);
    IRenderTarget CreateRenderTarget(TextureDesc textureDesc);
    IBufferGroup CreateBufferGroup();
}