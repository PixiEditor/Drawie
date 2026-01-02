using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Drawie.RenderApi.D3D11;

public class D3D11Texture : ITexture, IDisposable
{
    public ComPtr<ID3D11Texture2D> D3D11Handle { get; }

    public D3D11Texture(ComPtr<ID3D11Texture2D> d3d11Handle)
    {
        D3D11Handle = d3d11Handle;
    }

    public void Dispose()
    {
        D3D11Handle.Dispose();
    }
}
