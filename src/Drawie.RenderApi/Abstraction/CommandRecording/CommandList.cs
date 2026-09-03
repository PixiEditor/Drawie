using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.Abstraction.CommandRecording;

public abstract class CommandList : ICommandList
{
    private List<Action> instructions = new List<Action>();

    protected void RecordInstruction(Action instruction)
    {
        instructions.Add(instruction);
    }

    protected RecordedRenderPass ToRenderPass()
    {
        var execute = () =>
        {
            foreach (var instruction in instructions)
            {
                instruction.Invoke();
            }
        };
        return new RecordedRenderPass(execute);
    }

    protected void ClearInstructions()
    {
        instructions.Clear();
    }

    public abstract void BeginRenderPass(IRenderTarget fb);
    public abstract void SetPipeline(IPipeline pipeline);
    public abstract void SetViewportSize(float width, float height);

    public abstract void SetBuffers(IBufferGroup bufferGroup);
    public abstract void BindTexture(PreparedTexture texture, ISampler sampler);
    public abstract void DrawIndexed(int indexCount);
    public abstract void Draw(int vertexCount, int instanceCount);
    public abstract void Draw(int vertexCount, int instanceIndex, int instanceCount);

    public abstract RecordedRenderPass EndRenderPass(IRenderTarget blitTo);
    public abstract RecordedRenderPass EndRenderPass();
    public abstract RecordedRenderPass End();

    public abstract void BindPipeline();
    public abstract PreparedTexture PrepareTexture(ITexture texture);
    public abstract void UpdateUniforms(IEnumerable<UniformBlock> blocks, IEnumerable<PreparedTexture> textures, IEnumerable<ISampler> samplers);
    public abstract void UpdateUniforms(IEnumerable<NamedBuffer> properties);
    public abstract void RestoreTexture(PreparedTexture preparedTextureValue);
    public abstract void Blit(IRenderTarget renderTarget, IRenderTarget target);
}
