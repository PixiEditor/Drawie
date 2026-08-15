using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.RenderApi.WebGl.Enums;

public class WebGlCommandList : CommandList
{
    public int Gl { get; }
    private IRenderTarget source;

    private int originalFb;

    private int lastBoundTextureSlot = 0;

    public WebGlCommandList(int gl)
    {
        Gl = gl;
    }

    public override void BeginRenderPass(IRenderTarget fb)
    {
        source = fb;
        lastBoundTextureSlot = 0;
        ClearInstructions();
        RecordInstruction(() =>
        {
            originalFb = JSRuntime.GetParameter(Gl, (int)WebGlBindings.FramebufferBinding);
            JSRuntime.BindFramebuffer(Gl, (int)WebGlFramebufferTarget.Framebuffer, (int)fb.SurfaceId);
        });
    }

    public override void SetPipeline(IPipeline pipeline)
    {
        RecordInstruction(() => pipeline.Apply());
    }

    public override void SetBuffers(IBufferGroup bufferGroup)
    {
        RecordInstruction(() => { JSRuntime.BindVertexArray(Gl, (int)bufferGroup.Handle); });
    }

    public override void BindTexture(ITexture texture, ISampler sampler)
    {
        RecordInstruction(() =>
        {
            JSRuntime.ActiveTexture(Gl, (int)WebGlTextureUnit.Texture0 + lastBoundTextureSlot);
            JSRuntime.BindTexture(Gl, (int)WebGlTextureType.Texture2D, (int)texture.TextureId);
            JSRuntime.BindSampler(Gl, lastBoundTextureSlot, (int)sampler.Handle);
            lastBoundTextureSlot++;
        });
    }

    public override void DrawIndexed(int indexCount)
    {
        RecordInstruction(() =>
        {
            JSRuntime.DrawElements(Gl, (int)WebGlPrimitiveType.Triangles, indexCount,
                (int)WebGlDataType.UnsignedInt, 0);
        });
    }

    public override RecordedRenderPass EndRenderPass(IRenderTarget blitTo)
    {
        RecordInstruction(() =>
        {
            JSRuntime.BindFramebuffer(Gl, (int)WebGlFramebufferTarget.ReadFramebuffer, (int)source.SurfaceId);
            JSRuntime.BindFramebuffer(Gl, (int)WebGlFramebufferTarget.DrawFramebuffer, (int)blitTo.SurfaceId);
            JSRuntime.BlitFramebuffer(Gl,
                0, 0, source.Size.X, source.Size.Y,
                0, 0, blitTo.Size.X, blitTo.Size.Y,
                (int)WebGlBufferMask.ColorBufferBit,
                (int)WebGlTextureFilter.Nearest);
            JSRuntime.BindFramebuffer(Gl, (int)WebGlFramebufferTarget.Framebuffer, originalFb);
        });
        return ToRenderPass();
    }

    public override RecordedRenderPass EndRenderPass()
    {
        RecordInstruction(() => JSRuntime.BindFramebuffer(Gl, (int)WebGlFramebufferTarget.Framebuffer, originalFb));
        return ToRenderPass();
    }
}