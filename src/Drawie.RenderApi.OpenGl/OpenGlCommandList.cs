using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.OpenGL.Extensions;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlCommandList(GL api) : ICommandList
{
    public GL Api { get; } = api;

    private List<Action> instructions = new List<Action>();

    private IBuffer? vertexBuffer;
    private IBuffer? indexBuffer;
    private uint vao;

    public void BeginRenderPass(IRenderTarget fb)
    {
        instructions?.Clear();
        RecordInstruction(() => Api.BindFramebuffer(FramebufferTarget.Framebuffer, fb.FramebufferId));
    }

    public void SetPipeline(IPipeline pipeline)
    {
        RecordInstruction(() => pipeline.Apply());
    }

    public void SetVertexBuffer(IBuffer vertexBuffer)
    {
        this.vertexBuffer = vertexBuffer;
        RecordInstruction(() => Api.BindBuffer(vertexBuffer.Usage.ToOpenGlTargetARB(), vertexBuffer.NativeHandle));
        HandleVao();
    }

    public void SetIndexBuffer(IBuffer indexBuffer)
    {
        this.indexBuffer = indexBuffer;
        RecordInstruction(() => Api.BindBuffer(indexBuffer.Usage.ToOpenGlTargetARB(), indexBuffer.NativeHandle));
    }

    public void BindTexture(string textureName, ITexture texture)
    {
        throw new NotImplementedException();
    }

    public unsafe void DrawIndexed(int indexCount)
    {
        RecordInstruction(() => Api.DrawArrays(PrimitiveType.Triangles, 0, 3));
    }

    public RecordedRenderPass EndRenderPass()
    {
        RecordInstruction(() => Api.BindFramebuffer(FramebufferTarget.Framebuffer, 0));
        return new RecordedRenderPass(instructions.ToArray());
    }

    private void RecordInstruction(Action instruction)
    {
        instructions.Add(instruction);
    }

    private unsafe void HandleVao()
    {
        if (vertexBuffer == null)
            return;

        RecordInstruction(() =>
        {
            if (vao == 0)
            {
                vao = Api.GenVertexArray();
            }

            Api.BindVertexArray(vao);
            Api.BindBuffer(
                vertexBuffer.Usage.ToOpenGlTargetARB(),
                vertexBuffer.NativeHandle);

            Api.EnableVertexAttribArray(0);

            Api.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                3 * sizeof(float),
                null);
        });
    }
}