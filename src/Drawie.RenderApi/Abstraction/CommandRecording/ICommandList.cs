using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.Abstraction.CommandRecording;

public interface ICommandList
{
    void BeginRenderPass(IRenderTarget fb);
    void SetPipeline(IPipeline pipeline);
    void SetBuffers(IBufferGroup bufferGroup);
    void BindTexture(PreparedTexture texture, ISampler sampler);
    void DrawIndexed(int indexCount);
    RecordedRenderPass EndRenderPass(IRenderTarget blitTo);
    RecordedRenderPass EndRenderPass();
    void BindPipeline();
    PreparedTexture PrepareTexture(ITexture texture);
    void UpdateUniforms(List<UniformBlock> blocks, List<PreparedTexture> textures, List<ISampler> samplers);
}