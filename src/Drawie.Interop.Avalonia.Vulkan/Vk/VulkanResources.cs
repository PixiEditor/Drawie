using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanResources : RenderApiResources
{
    public VulkanInteropContext Context { get; }
    //public VulkanSwapchain Swapchain { get; }
    public override ITexture Texture => null;//Content.texture;

    public override bool IsDisposed => isDisposed;
    //public VulkanContent Content { get; }

    private CompositionDrawingSurface target;
    private ICompositionGpuInterop interop;
    private bool isDisposed;
    private VulkanSemaphorePair semaphorePair;

    private ICompositionImportedGpuSemaphore? availableSemaphore;
    private ICompositionImportedGpuSemaphore? renderCompletedSemaphore;

    public VulkanResources(CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop) : base(
        compositionDrawingSurface, interop)
    {
        Context = DrawieInterop.VulkanInteropContext;
        //Swapchain = new VulkanSwapchain(Context, interop, compositionDrawingSurface);
        //Content = new VulkanContent(Context);
        target = compositionDrawingSurface;
        this.interop = interop;
        semaphorePair = new VulkanSemaphorePair(Context, interop.SupportedImageHandleTypes, true);
    }

    public override async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        isDisposed = true;

        Context.Pool.FreeUsedCommandBuffers();
        //Content.Dispose();
        //await Swapchain.DisposeAsync();
    }

    public override void CreateTemporalObjects(PixelSize size)
    {
        if (isDisposed)
            return;

        //Content.CreateTemporalObjects(size, null);
    }

    public override void Render(PixelSize size, Func<IExportedTexture> renderAction)
    {
        if (isDisposed)
            return;

        using var buffer = Context.Pool.CreateCommandBuffer();
        buffer.BeginRecording();

        buffer.Submit(null, null, new[] { semaphorePair.RenderFinishedSemaphore });

        availableSemaphore ??= interop.ImportSemaphore(semaphorePair.Export(false));

        renderCompletedSemaphore ??= interop.ImportSemaphore(semaphorePair.Export(true));

        IExportedTexture texture = renderAction();
        if(texture == null)
            return;

        var imported = interop.ImportImage(new PlatformHandle(texture.NativeHandle, texture.Descriptor),
            new PlatformGraphicsExternalImageProperties
            {
                Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                Width = texture.Width,
                Height = texture.Height,
                MemoryOffset = 0,
                MemorySize = texture.MemorySize,
                TopLeftOrigin = true,
            });

        target.UpdateWithSemaphoresAsync(imported, renderCompletedSemaphore!, availableSemaphore!).ContinueWith(async (t) =>
        {
            //texture.Dispose();
            await imported.DisposeAsync();
        });
        /*using (Swapchain.BeginDraw(size, out var image))
        {
            var texture = renderAction();
            if (texture is not VulkanTexture vulkanTexture)
                throw new InvalidOperationException("Expected VulkanTexture from renderAction.");

            ImportAndBlit(image, vulkanTexture);
            Content.Render(image);
        }*/


    }

    private void ImportAndBlit(VulkanImage image, VulkanTexture texture)
    {
        var api = Context.Api;

        var commandBuffer = Context.Pool.CreateCommandBuffer();
        commandBuffer.BeginRecording();
        texture.TransitionLayoutTo(commandBuffer.InternalHandle, ImageLayout.ColorAttachmentOptimal,
            ImageLayout.TransferSrcOptimal);

        image.TransitionLayout(commandBuffer.InternalHandle, ImageLayout.TransferDstOptimal,
            AccessFlags.TransferWriteBit);

        var srcBlitRegion = new ImageBlit
        {
            SrcOffsets =
                new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
                },
            DstOffsets = new ImageBlit.DstOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0), Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
            },
            SrcSubresource =
                new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit, BaseArrayLayer = 0, LayerCount = 1, MipLevel = 0
                },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit, BaseArrayLayer = 0, LayerCount = 1, MipLevel = 0
            }
        };

        api.CmdBlitImage(commandBuffer.InternalHandle, texture.VkImage,
            ImageLayout.TransferSrcOptimal,
            image.InternalHandle, ImageLayout.TransferDstOptimal, 1, srcBlitRegion, Filter.Linear);

        commandBuffer.Submit();

        texture.TransitionLayoutTo((uint)ImageLayout.TransferSrcOptimal,
            (uint)ImageLayout.ColorAttachmentOptimal);
    }
}
