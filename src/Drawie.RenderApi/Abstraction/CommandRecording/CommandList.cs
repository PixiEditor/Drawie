using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
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
    public abstract void SetBuffers(IBufferGroup bufferGroup);
    public abstract void BindTexture(PreparedTexture texture, ISampler sampler);
    public abstract void DrawIndexed(int indexCount);
    public abstract RecordedRenderPass EndRenderPass(IRenderTarget blitTo);
    public abstract RecordedRenderPass EndRenderPass();
    public abstract void BindPipeline();
    public abstract PreparedTexture PrepareTexture(ITexture texture);
    public abstract void UpdateUniforms(List<UniformBlock> blocks, List<PreparedTexture> textures,
        List<ISampler> samplers);
}