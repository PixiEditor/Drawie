using Drawie.Backend.Shaders.Common;
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
    void Draw(int vertexCount, int instanceCount);
    void Draw(int vertexCount, int instanceIndex, int instanceCount);
    RecordedRenderPass EndRenderPass(IRenderTarget? blitTo);
    RecordedRenderPass EndRenderPass();
    RecordedRenderPass End();
    void BindPipeline();
    PreparedTexture PrepareTexture(ITexture texture);
    void UpdateUniforms(IEnumerable<UniformBlock> blocks, IEnumerable<PreparedTexture> textures, IEnumerable<ISampler> samplers);
    void UpdateUniforms(IEnumerable<NamedBuffer> properties);
    void RestoreTexture(PreparedTexture preparedTextureValue);
    public void Blit(IRenderTarget renderTarget, IRenderTarget target);
}
