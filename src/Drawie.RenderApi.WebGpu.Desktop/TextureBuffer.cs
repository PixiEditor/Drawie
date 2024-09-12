using Evergine.Bindings.WebGPU;
using PixiEditor.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Drawie.RenderApi.WebGpu;

public class TextureBuffer : IDisposable
{
    public WGPUTexture WgpuTexture { get; }
    public VecI Size { get; }

    private WGPUDevice Device { get; }

    public TextureBuffer(WGPUDevice device, WGPUQueue queue, VecI size)
    {
        Size = size;
        Device = device;

        unsafe
        {
            WGPUTextureDescriptor textureDescriptor = new WGPUTextureDescriptor()
            {
                nextInChain = null,
                label = null,
                size = new WGPUExtent3D()
                {
                    width = (uint)size.X,
                    height = (uint)size.Y,
                    depthOrArrayLayers = 1,
                },
                mipLevelCount = 1,
                sampleCount = 1,
                dimension = WGPUTextureDimension._2D,
                format = WGPUTextureFormat.RGBA8UnormSrgb,
                usage = WGPUTextureUsage.TextureBinding | WGPUTextureUsage.CopyDst,
            };

            WgpuTexture = WebGPUNative.wgpuDeviceCreateTexture(device, &textureDescriptor);

            BindTexture(device, queue, WgpuTexture);
        }
    }

    private unsafe void BindTexture(WGPUDevice device, WGPUQueue queue, WGPUTexture wgpuTexture)
    {
        WGPUImageCopyTexture imageCopyTexture = new WGPUImageCopyTexture()
        {
            texture = wgpuTexture,
            mipLevel = 0,
            origin = new WGPUOrigin3D()
            {
                x = 0,
                y = 0,
                z = 0,
            },
        };

        string path = Path.Combine("textur.png");

        var img = Image.Load<Rgba32>(path);
        ulong size = (ulong)(img.Width * img.Height * img.PixelType.BitsPerPixel / 8);

        WGPUBufferDescriptor stagingBufferDescriptor = new WGPUBufferDescriptor()
        {
            size = size,
            mappedAtCreation = true,
            usage = WGPUBufferUsage.CopySrc,
        };


        WGPUBuffer stagingBuffer = WebGPUNative.wgpuDeviceCreateBuffer(device, &stagingBufferDescriptor);

        void* data = WebGPUNative.wgpuBufferGetMappedRange(stagingBuffer, 0, size);
        img.CopyPixelDataTo(new Span<byte>(data, (int)size));
        WebGPUNative.wgpuBufferUnmap(stagingBuffer);
        img.Dispose();

        WGPUImageCopyBuffer imageCopyBuffer = new WGPUImageCopyBuffer()
        {
            buffer = stagingBuffer,
            layout = new WGPUTextureDataLayout()
            {
                offset = 0,
                bytesPerRow = (uint)img.Width * 4,
                rowsPerImage = (uint)img.Height,
            },
        };

        CopyTextureToBuffer(queue, imageCopyTexture, imageCopyBuffer);
        
        WebGPUNative.wgpuBufferDestroy(stagingBuffer);
    }

    private unsafe void CopyTextureToBuffer(WGPUQueue queue, WGPUImageCopyTexture imageCopyTexture,
        WGPUImageCopyBuffer imageCopyBuffer)
    {
        WGPUCommandEncoderDescriptor commandEncoderDescriptor = new WGPUCommandEncoderDescriptor()
        {
            label = null,
        };

        WGPUCommandEncoder commandEncoder =
            WebGPUNative.wgpuDeviceCreateCommandEncoder(Device, &commandEncoderDescriptor);

        WGPUExtent3D extent3D = new WGPUExtent3D()
        {
            width = (uint)Size.X,
            height = (uint)Size.Y,
            depthOrArrayLayers = 1,
        };

        WebGPUNative.wgpuCommandEncoderCopyBufferToTexture(commandEncoder, &imageCopyBuffer, &imageCopyTexture,
            &extent3D);

        WGPUCommandBufferDescriptor commandBufferDescriptor = new WGPUCommandBufferDescriptor()
        {
            label = null,
        };

        WGPUCommandBuffer commandBuffer =
            WebGPUNative.wgpuCommandEncoderFinish(commandEncoder, &commandBufferDescriptor);

        WebGPUNative.wgpuQueueSubmit(queue, 1, &commandBuffer);
    }

    public void Dispose()
    {
        WebGPUNative.wgpuTextureDestroy(WgpuTexture);
    }
}