using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Drawie.Interop.Avalonia.D3D11;

public class D3D11RenderApiResources : RenderApiResources
{
    public override ITexture Texture { get; }
    public override bool IsDisposed { get; }

    private Device? _device;
    private D3D11Swapchain? _swapchain;
    private DeviceContext? _context;


    public D3D11RenderApiResources(CompositionDrawingSurface surface, ICompositionGpuInterop gpuInterop) : base(surface,
        gpuInterop)
    {
        if (!gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes
                .D3D11TextureGlobalSharedHandle)
           )
        {
            throw new PlatformNotSupportedException(
                "D3D11TextureGlobalSharedHandle not supported by this interop"
            );
        }

        var factory = new SharpDX.DXGI.Factory1();
        using var adapter = factory.GetAdapter1(0);
        _device = new Device(
            adapter,
            DeviceCreationFlags.None,
            new[]
            {
                FeatureLevel.Level_12_1, FeatureLevel.Level_12_0, FeatureLevel.Level_11_1, FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_0, FeatureLevel.Level_9_3, FeatureLevel.Level_9_2, FeatureLevel.Level_9_1,
            }
        );
        _swapchain = new D3D11Swapchain(_device, gpuInterop, surface);
        _context = _device.ImmediateContext;
    }


    public override ValueTask DisposeAsync()
    {
        if (_swapchain is not null)
        {
            _swapchain.DisposeAsync().GetAwaiter().GetResult();
            _swapchain = null;
        }

        Utilities.Dispose(ref _context);
        Utilities.Dispose(ref _device);

        return ValueTask.CompletedTask;
    }

    public override void CreateTemporalObjects(PixelSize size)
    {
        if (_device is null)
            return;

        // Setup targets and viewport for rendering
        _device.ImmediateContext.Rasterizer.SetViewport(
            new Viewport(0, 0, size.Width, size.Height, 0.0f, 1.0f)
        );
    }

    public override void Render(PixelSize size, Action renderAction)
    {
        using (_swapchain!.BeginDraw(size, out var image))
        {
            _device!.ImmediateContext.OutputMerger.SetTargets(image.RenderTargetView);
            var context = _device.ImmediateContext;

            // Clear views
            context.ClearRenderTargetView(image.RenderTargetView, new RawColor4(1, 0, 0, 1));

            renderAction();

            _context!.Flush();
        }
    }
}
