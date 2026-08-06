using Drawie.Backend.Core;

namespace Drawie.Rendering;

/// <summary>
///     Readonly texture, can be modified with a Rendering Context
/// </summary>
public class Texture
{
    internal NativeTexture NativeTexture { get; }
    public TextureState State { get; internal set; } = TextureState.Submitted;
    public int Width { get; }
    public int Height { get; }

    internal Texture(NativeTexture native)
    {
        NativeTexture = native;
        Width = native.Size.X;
        Height = native.Size.Y;
    }
}

public enum TextureState
{
    Editing,
    WaitingForSubmit,
    Submitting,
    Submitted,
}