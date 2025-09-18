using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.OpenGl;

public class OpenGlSwapchain : SwapchainBase<IGlSwapchainImage>
{
    private readonly IGlContext _context;
    private readonly IGlContextExternalObjectsFeature? _externalObjectsFeature;
    private readonly IOpenGlTextureSharingRenderInterfaceContextFeature? _sharingFeature;

    public OpenGlSwapchain(IGlContext context, ICompositionGpuInterop interop,
        CompositionDrawingSurface target,
        IOpenGlTextureSharingRenderInterfaceContextFeature sharingFeature
    ) : base(interop, target)
    {
        _context = context;
        _sharingFeature = sharingFeature;
    }

    public OpenGlSwapchain(IGlContext context, ICompositionGpuInterop interop,
        CompositionDrawingSurface target,
        IGlContextExternalObjectsFeature? externalObjectsFeature) : base(interop, target)
    {
        _context = context;
        _externalObjectsFeature = externalObjectsFeature;
    }


    public override IGlSwapchainImage CreateImage(VecI size)
    {
        if (_sharingFeature != null)
            return new CompositionOpenGlSwapChainImage(_context, _sharingFeature, size, Interop, Target);
        return new DxgiMutexOpenGlSwapChainImage(Interop, Target, _externalObjectsFeature!, size);
    }

    public Frame BeginDraw(VecI size, out IOpenGlTexture texture)
    {
        var rv = BeginDrawCore(size, out var tex);
        texture = tex;
        return new Frame() { PresentFrame = rv.present, ReturnFrame = rv.returnToPool, Size = size, Texture = tex };
    }
}

public interface IGlSwapchainImage : ISwapchainImage, IOpenGlTexture
{
}

internal class DxgiMutexOpenGlSwapChainImage : IGlSwapchainImage
{
    private readonly ICompositionGpuInterop _interop;
    private readonly CompositionDrawingSurface _surface;
    private readonly IGlExportableExternalImageTexture _texture;
    private Task? _lastPresent;
    private ICompositionImportedGpuImage? _imported;

    public DxgiMutexOpenGlSwapChainImage(ICompositionGpuInterop interop, CompositionDrawingSurface surface,
        IGlContextExternalObjectsFeature externalObjects, VecI size)
    {
        _interop = interop;
        _surface = surface;
        _texture = externalObjects.CreateImage(
            KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle,
            new PixelSize(size.X, size.Y), PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm);
    }

    public async ValueTask DisposeAsync()
    {
        // The texture is already sent to the compositor, so we need to wait for its attempts to use the texture
        // before destroying it
        if (_imported != null)
        {
            // No need to wait for import / LastPresent since calls are serialized on the compositor side anyway
            try
            {
                await _imported.DisposeAsync();
            }
            catch
            {
                // Ignore
            }
        }

        _texture.Dispose();
    }

    public uint TextureId => (uint)_texture.TextureId;
    public int InternalFormat => _texture.InternalFormat;
    public VecI Size => new(_texture.Properties.Width, _texture.Properties.Height);
    public void BlitFrom(ITexture texture)
    {
        throw new NotImplementedException();
    }

    public void BlitFrom(ITexture backingBackbufferTexture, object? renderFinishedSemaphore,
        object? blitSignalSemaphore)
    {
        throw new NotImplementedException();
    }

    public Task? LastPresent => _lastPresent;
    public void BeginDraw() => _texture.AcquireKeyedMutex(0);

    public Task Present(VecI size)
    {
        _texture.ReleaseKeyedMutex(1);
        _imported ??= _interop.ImportImage(_texture.GetHandle(), _texture.Properties);
        _lastPresent = _surface.UpdateWithKeyedMutexAsync(_imported, 1, 0);
        return _lastPresent;
    }

    public FrameHandle ExportFrame()
    {
        throw new NotImplementedException();
    }
}

internal class CompositionOpenGlSwapChainImage : IGlSwapchainImage
{
    private readonly ICompositionGpuInterop _interop;
    private readonly CompositionDrawingSurface _target;
    private readonly ICompositionImportableOpenGlSharedTexture _texture;
    private ICompositionImportedGpuImage? _imported;

    public CompositionOpenGlSwapChainImage(
        IGlContext context,
        IOpenGlTextureSharingRenderInterfaceContextFeature sharingFeature,
        VecI size,
        ICompositionGpuInterop interop,
        CompositionDrawingSurface target)
    {
        _interop = interop;
        _target = target;
        _texture = sharingFeature.CreateSharedTextureForComposition(context, new PixelSize(size.X, size.Y));
    }


    public async ValueTask DisposeAsync()
    {
        // The texture is already sent to the compositor, so we need to wait for its attempts to use the texture
        // before destroying it
        if (_imported != null)
        {
            // No need to wait for import / LastPresent since calls are serialized on the compositor side anyway
            try
            {
                await _imported.DisposeAsync();
            }
            catch
            {
                // Ignore
            }
        }

        _texture.Dispose();
    }

    public uint TextureId => (uint)_texture.TextureId;
    public int InternalFormat => _texture.InternalFormat;
    public VecI Size => new VecI(_texture.Size.Width, _texture.Size.Height);
    public void BlitFrom(ITexture texture)
    {
        throw new NotImplementedException();
    }

    public void BlitFrom(ITexture backingBackbufferTexture, object? renderFinishedSemaphore,
        object? blitSignalSemaphore)
    {
        throw new NotImplementedException();
    }

    public Task? LastPresent { get; private set; }

    public void BeginDraw()
    {
        // No-op for texture sharing
    }

    public Task Present(VecI size)
    {
        _imported ??= _interop.ImportImage(_texture);
        LastPresent = _target.UpdateAsync(_imported);
        return LastPresent;
    }

    public FrameHandle ExportFrame()
    {
        throw new NotImplementedException();
    }
}
