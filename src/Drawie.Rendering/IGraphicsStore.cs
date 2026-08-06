using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Rendering;

/// <summary>
///     Represents a structure holding and managing native textures and objects
/// </summary>
public class GraphicsStore : IDisposable
{
    public IGraphicsContext GraphicsContext { get; set; }
    private List<Texture> textures = new List<Texture>();
    
    public GraphicsStore(IGraphicsContext graphicsContext)
    {
        GraphicsContext = graphicsContext;
    }
    
    public Texture CreateNativeRenderSurface(VecI size, ITexture nativeTexture, SurfaceOrigin origin)
    {
        if (!GraphicsContext.OwnsTexture(nativeTexture))
        {
            throw new ArgumentException("The given native texture is not owned by this graphics store.");
        }
        
        textures.Add(new Texture(NativeTexture.FromExisting(DrawingBackendApi.Current.CreateRenderSurface(size, nativeTexture, origin))));
        return textures[^1];
    }

    public void Dispose()
    {
        foreach (var texture in textures)
        {
            texture.NativeTexture.Dispose();
        }
    }
}