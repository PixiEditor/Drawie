using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
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
    private IPipeline boundPipeline;

    public override void BeginRenderPass(IRenderTarget fb)
    {
        source = fb;
        lastBoundTextureSlot = 0;
        ClearInstructions();
        RecordInstruction(() =>
        {
            originalFb = (uint)Api.GetInteger(GLEnum.FramebufferBinding);
            Api.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)fb.SurfaceId);
        });
    }

    public override void SetPipeline(IPipeline pipeline)
    {
        boundPipeline = pipeline;
    }
    
    public override void BindPipeline()
    {
        RecordInstruction(() => boundPipeline.Apply(this));
    }

    public override PreparedTexture PrepareTexture(ITexture texture)
    {
        return new PreparedTexture(texture.TextureId);
    }

    public override void UpdateUniforms(List<UniformBlock> blocks, List<PreparedTexture> textures, List<ISampler> samplers)
    {
        
    }

    public override void RestoreTexture(PreparedTexture preparedTextureValue)
    {
        // no op
    }

    public override void SetBuffers(IBufferGroup bufferGroup)
    {
        RecordInstruction(() => { Api.BindVertexArray(bufferGroup.Handle); });
    }

    public override void BindTexture(PreparedTexture texture, ISampler sampler)
    {
        if (sampler is not OpenGlSampler openGlSampler) throw new ArgumentException("Cannot bind non opengl samplers");
        // if vk:binding has binding set to 1, we need to update it at Texture1, generally, for binding transformation matrices we want to use 
        // binding 0, so binding 1 is a good assumption. Ideally shader reflection can resolve that but I guess it's fine for now

        RecordInstruction(() =>
        {
            int textureUnit = lastBoundTextureSlot + 1;
            Api.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            Api.BindTexture(TextureTarget.Texture2D, (uint)texture.Handle);
            Api.BindSampler((uint)textureUnit, openGlSampler.Handle);
            lastBoundTextureSlot++;
        });
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
            Api.BindFramebuffer(FramebufferTarget.ReadFramebuffer, (uint)source.SurfaceId);
            Api.BindFramebuffer(FramebufferTarget.DrawFramebuffer, (uint)blitTo.SurfaceId);
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
