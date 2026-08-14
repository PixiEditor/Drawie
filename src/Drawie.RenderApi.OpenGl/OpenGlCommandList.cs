using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.OpenGL.Extensions;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlCommandList(GL api) : CommandList
{
    public GL Api { get; } = api;

    private IRenderTarget source;

    private uint originalFb;

    private int lastBoundTextureSlot = 0;

    public override void BeginRenderPass(IRenderTarget fb)
    {
        source = fb;
        lastBoundTextureSlot = 0;
        ClearInstructions();
        RecordInstruction(() =>
        {
            originalFb = (uint)Api.GetInteger(GLEnum.FramebufferBinding);
            Api.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)fb.FramebufferId);
        });
    }

    public override void SetPipeline(IPipeline pipeline)
    {
        RecordInstruction(pipeline.Apply);
    }

    public override void SetBuffers(IBufferGroup bufferGroup)
    {
        RecordInstruction(() => { Api.BindVertexArray(bufferGroup.Handle); });
    }

    public override void BindTexture(ITexture texture, ISampler sampler)
    {
        if (sampler is not OpenGlSampler openGlSampler) throw new ArgumentException("Cannot bind non opengl samplers");
        Api.ActiveTexture(TextureUnit.Texture0 + lastBoundTextureSlot);
        Api.BindTexture(TextureTarget.Texture2D, (uint)texture.TextureId);
        Api.BindSampler((uint)lastBoundTextureSlot, openGlSampler.Handle);
        lastBoundTextureSlot++;
    }

    public override unsafe void DrawIndexed(int indexCount)
    {
        RecordInstruction(() =>
        {
            Api.DrawElements(PrimitiveType.Triangles, (uint)indexCount, DrawElementsType.UnsignedInt, (void*)0);
        });
    }

    public override RecordedRenderPass EndRenderPass(IRenderTarget blitTo)
    {
        RecordInstruction(() =>
        {
            Api.BindFramebuffer(FramebufferTarget.ReadFramebuffer, (uint)source.FramebufferId);
            Api.BindFramebuffer(FramebufferTarget.DrawFramebuffer, (uint)blitTo.FramebufferId);
            Api.BlitFramebuffer(
                0, 0, source.Size.X, source.Size.Y,
                0, 0, blitTo.Size.X, blitTo.Size.Y,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Nearest);
            Api.BindFramebuffer(FramebufferTarget.Framebuffer, originalFb);
        });
        return ToRenderPass();
    }

    public override RecordedRenderPass EndRenderPass()
    {
        RecordInstruction(() => Api.BindFramebuffer(FramebufferTarget.Framebuffer, originalFb));
        return ToRenderPass();
    }
}