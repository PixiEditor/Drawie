using Avalonia;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;

namespace Drawie.Interop.VulkanAvalonia.Controls;

public class DrawieTextureControl : DrawieControl
{
    public static readonly StyledProperty<Texture> TextureProperty = AvaloniaProperty.Register<DrawieTextureControl, Texture>(
        nameof(Texture));

    public Texture Texture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }
    static DrawieTextureControl()
    {
        AffectsRender<DrawieTextureControl>(TextureProperty);
    }
    
    public override void Draw(DrawingSurface surface)
    {
        if (Texture == null)
            return;

        surface.Canvas.Clear(Colors.Transparent);
        surface.Canvas.DrawSurface(Texture.DrawingSurface, 0, 0);
    }
}