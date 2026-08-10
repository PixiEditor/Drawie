using Drawie.Backend.Core;
using Drawie.Numerics;

namespace Drawie.Rendering;

/// <summary>
///     Readonly texture, can be modified with a Rendering Context
/// </summary>
public class Texture
{
    internal NativeTexture NativeTexture { get; }
    public int Width { get; }
    public int Height { get; }
    public VecI Size => new VecI(Width, Height);

    internal Texture(NativeTexture native)
    {
        NativeTexture = native;
        Width = native.Size.X;
        Height = native.Size.Y;
    }
}