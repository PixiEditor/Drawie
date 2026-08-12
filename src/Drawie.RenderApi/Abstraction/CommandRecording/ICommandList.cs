using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.Abstraction.CommandRecording;

public interface ICommandList
{
    void BeginRenderPass(IRenderTarget fb);
    void SetPipeline(IPipeline pipeline);
    void SetBuffers(IBufferGroup bufferGroup);
    void BindTexture(ITexture texture, ISampler sampler);
    void DrawIndexed(int indexCount);
    RecordedRenderPass EndRenderPass(IRenderTarget blitTo);
    RecordedRenderPass EndRenderPass();
}