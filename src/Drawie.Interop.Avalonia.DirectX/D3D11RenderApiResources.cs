using System.Numerics;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;
using Drawie.RenderApi.D3D11;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Drawie.Interop.Avalonia.DirectX;

public class D3D11RenderApiResources : RenderApiResources
{
    private ComPtr<ID3D11Device> _device;
    private D3D11Swapchain? _swapchain;
    private ComPtr<ID3D11DeviceContext> _context;
    private PixelSize _lastSize;
    private D3D11Texture? _texture;
    private ComPtr<ID3D11RenderTargetView> _rtv;
    private bool isDisposed;

    public override ITexture Texture => _texture;
    public override bool IsDisposed => isDisposed;

    public unsafe D3D11RenderApiResources(
        CompositionDrawingSurface surface,
        ICompositionGpuInterop interop,
        ComPtr<ID3D11Device> device,
        ComPtr<ID3D11DeviceContext> context) : base(
        surface, interop)
    {
        _device = device;
        _swapchain = new D3D11Swapchain(device, interop, surface);
        _context = context;
    }


    public override async ValueTask DisposeAsync()
    {
        if (_swapchain is not null)
        {
            await _swapchain.DisposeAsync();
            _swapchain = null;
        }
    }

    public override unsafe void CreateTemporalObjects(PixelSize size)
    {
        if (size == _lastSize)
            return;

        DestroyTemporalObjects();

        var desc = new Texture2DDesc
        {
            Width = (uint)size.Width,
            Height = (uint)size.Height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)BindFlag.RenderTarget,
            CPUAccessFlags = 0,
            MiscFlags = (uint)ResourceMiscFlag.Shared
        };

        ComPtr<ID3D11Texture2D> image = default;
        SilkMarshal.ThrowHResult(_device.CreateTexture2D(&desc, null, ref image));
        SilkMarshal.ThrowHResult(_device.CreateRenderTargetView(image, null, ref _rtv));

        _texture = new D3D11Texture(image);
        _lastSize = size;
    }

    public override unsafe void Render(PixelSize pixelSize, Action renderAction)
    {
        if (pixelSize == default)
            return;
        if (pixelSize != _lastSize)
        {
            CreateTemporalObjects(pixelSize);
            _lastSize = pixelSize;
        }

        using (_swapchain!.BeginDraw(pixelSize, out ComPtr<ID3D11RenderTargetView> view))
        {
            var rtv = _rtv.Handle;
            _context.OMSetRenderTargets(1, &rtv, (ID3D11DepthStencilView*)null);
            renderAction();
            using ComPtr<ID3D11Resource> dstRes = default;
            view.GetResource(dstRes.GetAddressOf());

            _context.CopyResource(dstRes, _texture.D3D11Handle);
            _context.Flush();
        }
    }

    public void DestroyTemporalObjects()
    {
        _rtv.Dispose();
        _texture?.Dispose();
        _lastSize = new PixelSize();
    }
}
