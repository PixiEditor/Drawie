using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public class SoftwareRenderApiResources : RenderApiResources
{
    private AvaloniaBitmapTexture texture;
    public override ITexture Texture => texture;
    public override bool IsDisposed { get; }
    public override ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public SoftwareRenderApiResources(InteropData data) : base(data)
    {

    }

    public override void CreateTemporalObjects(PixelSize size)
    {
        texture?.Bitmap?.Dispose();
        texture = new AvaloniaBitmapTexture(size);
    }

    public override void Render(PixelSize size, Action renderAction)
    {
        renderAction();
    }
}
