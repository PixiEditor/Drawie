using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;
using Drawie.Backend.Core.Utils;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Drawie.Interop.Avalonia.DirectX;

public class D3D11InteropContext : ID3D11Context, IDrawieInteropContext
{
    public ComPtr<ID3D11Device> Device { get; }
    public ComPtr<ID3D11DeviceContext> Context { get; }
    public ComPtr<IDXGIAdapter> Adapter { get; }
    public D3DFeatureLevel FeatureLevel { get; }

    private string? deviceDesc;

    public unsafe D3D11InteropContext()
    {
        using var dxgi = new DXGI(DXGI.CreateDefaultContext(["DXGI.dll"]));
        using var d3d11 = new D3D11(D3D11.CreateDefaultContext(["d3d11.dll"]));
        using var factory = dxgi.CreateDXGIFactory1<IDXGIFactory1>();

        using ComPtr<IDXGIAdapter> adapter = default;
        SilkMarshal.ThrowHResult(factory.EnumAdapters(0, adapter.GetAddressOf()));

        const int featureLevelCount = 8;
        var featureLevels = stackalloc D3DFeatureLevel[featureLevelCount]
        {
            D3DFeatureLevel.Level121, D3DFeatureLevel.Level120, D3DFeatureLevel.Level111, D3DFeatureLevel.Level110,
            D3DFeatureLevel.Level100, D3DFeatureLevel.Level93, D3DFeatureLevel.Level92, D3DFeatureLevel.Level91
        };

        ComPtr<ID3D11Device> device = default;
        ComPtr<ID3D11DeviceContext> context = default;
        D3DFeatureLevel actualFeatureLevel;
        const uint D3D11_CREATE_DEVICE_BGRA_SUPPORT = 0x20;

        SilkMarshal.ThrowHResult(d3d11.CreateDevice(
            adapter,
            D3DDriverType.Unknown,
            IntPtr.Zero,
            D3D11_CREATE_DEVICE_BGRA_SUPPORT,
            featureLevels,
            featureLevelCount,
            D3D11.SdkVersion,
            device.GetAddressOf(),
            &actualFeatureLevel,
            context.GetAddressOf()));

        Device = device;
        Context = context;
        Adapter = adapter;

        AdapterDesc adapterDesc;
        SilkMarshal.ThrowHResult(adapter.GetDesc(&adapterDesc));
        var description = SilkMarshal.PtrToString((IntPtr)adapterDesc.Description, NativeStringEncoding.LPWStr);
        deviceDesc = description;

        FeatureLevel = actualFeatureLevel;
    }

    public RenderApiResources CreateResources(CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        if (!interop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes
                .D3D11TextureGlobalSharedHandle))
            throw new Exception("DXGI shared handle import is not supported by the current graphics backend");

        return new D3D11RenderApiResources(surface, interop, Device, Context);
    }

    public GpuDiagnostics GetGpuDiagnostics()
    {
        return new GpuDiagnostics(
            true,
            new GpuInfo(deviceDesc, null /*todo*/),
            $"DirectX ({FeatureLevel})", new Dictionary<string, string>());
    }

    public IDisposable EnsureContext()
    {
        return Disposable.Empty;
    }

    unsafe IntPtr ID3D11Context.Device => (IntPtr)Device.Handle;
    unsafe IntPtr ID3D11Context.Adapter => (IntPtr)Adapter.Handle;
}
