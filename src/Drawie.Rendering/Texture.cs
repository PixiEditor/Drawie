using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Rendering;

/// <summary>
///     Readonly texture, can be modified with a Rendering Context
/// </summary>
public class Texture : ITexture
{
    public NativeTexture NativeTexture { get; }
    public int Width { get; }
    public int Height { get; }
    public VecI Size => new VecI(Width, Height);
    public ulong TextureId  => NativeTexture.TextureId;


    internal Texture(NativeTexture native)
    {
        NativeTexture = native;
        Width = native.Size.X;
        Height = native.Size.Y;
    }
}