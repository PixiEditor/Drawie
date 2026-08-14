using System.Text;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Rendering;

/// <summary>
///     Represents a structure holding and managing native textures and objects
/// </summary>
public class GraphicsStore : IDisposable
{
    public static GraphicsStore Global { get; set; }
#if DEBUG
    public static List<GraphicsStore> AllStores = new List<GraphicsStore>();
#endif
    public IGraphicsContext GraphicsContext { get; set; }
    private List<Texture> textures = new List<Texture>();

    public bool IsDisposed { get; private set; }

    public GraphicsStore(IGraphicsContext graphicsContext)
    {
        GraphicsContext = graphicsContext;
#if DEBUG
        AllStores.Add(this);
#endif
    }

    public Texture Create(VecI size)
    {
        textures.Add(new Texture(new NativeTexture(size)));
        return textures[^1];
    }

    public Texture CreateNativeRenderSurface(VecI size, ITexture nativeTexture, SurfaceOrigin origin)
    {
        textures.Add(new Texture(
            NativeTexture.FromExisting(DrawingBackendApi.Current.CreateRenderSurface(size, nativeTexture, origin))));
        return textures[^1];
    }
    
    public void DisposeTexture(Texture toDispose)
    {
        if(!textures.Contains(toDispose))
        {
            throw new ArgumentException("The given texture is not owned by this graphics store.");
        }
        
        textures.Remove(toDispose);
        toDispose.NativeTexture.Dispose();
    }

    public void Dispose()
    {
        foreach (var texture in textures)
        {
            texture.NativeTexture.Dispose();
        }

        textures.Clear();
        IsDisposed = true;
    }

#if DEBUG
    public string GetDebugText()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("GraphicsStore Debug Info:");
        sb.AppendLine("Textures Count: " + textures.Count);
        int totalMemoryMb = CountMemoryUsed(textures);
        sb.AppendLine("Total Memory Used: " + totalMemoryMb + "MB");
        return sb.ToString();
    }

    private int CountMemoryUsed(List<Texture> list)
    {
        int bytes = 0;
        foreach (var texture in list)
        {
            bytes += texture.NativeTexture.ImageInfo.BytesSize;
        }

        return bytes / (1024 * 1024);
    }
#endif
}