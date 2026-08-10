using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;

namespace Drawie.RenderApi.Abstraction.CommandRecording;

public interface ICommandList
{
    void BeginRenderPass(IRenderTarget fb);
    void SetPipeline(IPipeline pipeline);
    void SetVertexBuffer(IBuffer vertexBuffer);
    void SetIndexBuffer(IBuffer indexBuffer);
    void BindTexture(string textureName, ITexture texture);
    void DrawIndexed(int indexCount);
    RecordedRenderPass EndRenderPass(IRenderTarget blitTo);
    RecordedRenderPass EndRenderPass();
}