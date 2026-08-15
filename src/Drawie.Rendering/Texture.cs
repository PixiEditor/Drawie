using Drawie.Backend.Core;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Rendering;

/// <summary>
///     Readonly texture, can be modified with a Rendering Context
/// </summary>
public class Texture : ITexture, IDisposable
{
    internal NativeTexture NativeTexture { get; }
    public int Width { get; }
    public int Height { get; }
    public VecI Size => new VecI(Width, Height);
    public ulong TextureId  => NativeTexture.TextureId;

    public Texture(VecI size) : this(new NativeTexture(size))
    {
    }

    public Texture(NativeTexture native)
    {
        NativeTexture = native;
        Width = native.Size.X;
        Height = native.Size.Y;
    }

    public void Dispose()
    {
        NativeTexture.Dispose();
    }
}