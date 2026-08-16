using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanRenderTarget(VulkanContext context, Buffers.VulkanTexture texture, VecI size) : IRenderTarget, IDisposable
{
    public VulkanContext Context { get; } = context;
    public ulong SurfaceId => Texture.ImageHandle;
    public VecI Size { get; } = size;
    public Buffers.VulkanTexture Texture { get; } = texture;

    public Framebuffer? Framebuffer { get; private set; }

    private RenderPass? lastCreatedRenderPass;

    public unsafe void CreateFramebufferFor(RenderPass renderPass)
    {
        if (lastCreatedRenderPass != null && lastCreatedRenderPass.Value.Handle == renderPass.Handle) return;

        DestroyFramebuffer();
        ImageView* attachments = stackalloc ImageView[(int)Texture.Attachments];

        attachments[0] = Texture.ColorAttachment.View;
        int index = 1;
        if (Texture.DepthAttachment != null)
        {
            attachments[index] = Texture.DepthAttachment.View;
            index++;
        }

        if (Texture.MsaaResolvedColorAttachment != null)
        {
            attachments[index] = Texture.MsaaResolvedColorAttachment.View;
        }

        FramebufferCreateInfo framebufferCreateInfo = new()
        {
            SType = StructureType.FramebufferCreateInfo,
            Width = (uint)Size.X,
            Height = (uint)Size.Y,
            RenderPass = renderPass,
            AttachmentCount = Texture.Attachments,
            PAttachments = attachments,
            Layers = 1
        };

        Context.Api.CreateFramebuffer(Context.LogicalDevice.Device, &framebufferCreateInfo, null, out var framebuffer)
            .ThrowOnError("Failed to create framebuffer");
        Framebuffer = framebuffer;
    }

    private unsafe void DestroyFramebuffer()
    {
        if(Framebuffer != null)
            Context.Api.DestroyFramebuffer(Context.LogicalDevice.Device, Framebuffer.Value, null);
    }

    public void Dispose()
    {
        Texture.Dispose();
    }
}