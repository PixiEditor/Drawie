using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.OpenGL.Extensions;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlCommandList(GL api) : ICommandList
{
    public GL Api { get; } = api;

    private List<Action> instructions = new List<Action>();
    private IRenderTarget source;

    private IBuffer? vertexBuffer;
    private IBuffer? indexBuffer;
    private uint vao;
    private uint originalFb;

    public void BeginRenderPass(IRenderTarget fb)
    {
        source = fb;
        instructions?.Clear();
        RecordInstruction(() =>
        {
            originalFb = (uint)Api.GetInteger(GLEnum.FramebufferBinding);
            Api.BindFramebuffer(FramebufferTarget.Framebuffer, fb.FramebufferId);
        });
    }

    public void SetPipeline(IPipeline pipeline)
    {
        RecordInstruction(() => pipeline.Apply());
    }

    public void SetVertexBuffer(IBuffer vertexBuffer)
    {
        this.vertexBuffer = vertexBuffer;
        Api.BindBuffer(vertexBuffer.Usage.ToOpenGlTargetARB(), vertexBuffer.NativeHandle);
        HandleVao();
        RecordInstruction(() => Api.BindVertexArray(vao));
    }

    public void SetIndexBuffer(IBuffer indexBuffer)
    {
        this.indexBuffer = indexBuffer;
        Api.BindBuffer(indexBuffer.Usage.ToOpenGlTargetARB(), indexBuffer.NativeHandle);
    }

    public void BindTexture(ITexture texture, ISampler sampler)
    {
        if (texture is not IOpenGlTexture openGlTexture) throw new ArgumentException("Cannot bind non opengl textures");
        if (sampler is not OpenGlSampler openGlSampler) throw new ArgumentException("Cannot bind non opengl samplers");
        Api.ActiveTexture(TextureUnit.Texture0);
        Api.BindTexture(TextureTarget.Texture2D, openGlTexture.TextureId);
        //Api.BindSampler(0, openGlSampler.SamplerId);
    }

    public void DrawIndexed(int indexCount)
    {
        RecordInstruction(() => Api.DrawArrays(PrimitiveType.Triangles, 0, 36));
    }

    public RecordedRenderPass EndRenderPass(IRenderTarget blitTo)
    {
        RecordInstruction(() =>
        {
            Api.BindFramebuffer(FramebufferTarget.ReadFramebuffer, source.FramebufferId);
            Api.BindFramebuffer(FramebufferTarget.DrawFramebuffer, blitTo.FramebufferId);
            Api.BlitFramebuffer(
                0, 0, source.Size.X, source.Size.Y,
                0, 0, blitTo.Size.X, blitTo.Size.Y,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            Api.BindFramebuffer(FramebufferTarget.Framebuffer, originalFb);
        });
        return new RecordedRenderPass(instructions.ToArray());
    }

    public RecordedRenderPass EndRenderPass()
    {
        RecordInstruction(() => Api.BindFramebuffer(FramebufferTarget.Framebuffer, originalFb));
        return new RecordedRenderPass(instructions.ToArray());
    }

    private void RecordInstruction(Action instruction)
    {
        instructions.Add(instruction);
    }

    private unsafe void HandleVao()
    {
        if (vertexBuffer == null || indexBuffer == null)
            return;

        if (vao == 0)
        {
            vao = Api.GenVertexArray();
        }

        Api.BindVertexArray(vao);
        Api.BindBuffer(
            vertexBuffer.Usage.ToOpenGlTargetARB(),
            vertexBuffer.NativeHandle);

        Api.BindBuffer(indexBuffer.Usage.ToOpenGlTargetARB(),
            indexBuffer?.NativeHandle ?? 0);

        VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
        VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
        VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
    }
    
    private unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
        int offset)
    {
        int vTypeSize = sizeof(float);
        Api.VertexAttribPointer(index, count, type, false, vertexSize * (uint) vTypeSize, (void*) (offset * vTypeSize));
        Api.EnableVertexAttribArray(index);
    }
}