using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace Drawie.Interop.Avalonia.Core.Controls;

public class DrawieTextureControl : DrawieControl
{
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<DrawieTextureControl, Stretch>(
            nameof(Stretch), Stretch.Uniform);

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly StyledProperty<Texture> TextureProperty =
        AvaloniaProperty.Register<DrawieTextureControl, Texture>(
            nameof(Texture));

    public Texture Texture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    private Texture? texture;
    private Stretch stretch = Stretch.Uniform;
    private Rect bounds;

    static DrawieTextureControl()
    {
        AffectsRender<DrawieTextureControl>(TextureProperty, StretchProperty);
        AffectsMeasure<DrawieTextureControl>(TextureProperty, StretchProperty);
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = Texture;
        var result = new Size();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, new Size(source.Size.X, source.Size.Y));
        }
        else if (Width > 0 && Height > 0)
        {
            result = Stretch.CalculateSize(availableSize, new Size(Width, Height));
        }

        return result;
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var source = Texture;

        if (source != null)
        {
            var sourceSize = source.Size;
            var result = Stretch.CalculateSize(finalSize, new Size(sourceSize.X, sourceSize.Y));
            return result;
        }
        else
        {
            return Stretch.CalculateSize(finalSize, new Size(Width, Height));
        }

        return new Size();
    }

    protected override void PrepareToDraw()
    {
        texture = Texture;
        stretch = Stretch;
        bounds = Bounds;
    }

    public override void Draw(DrawingSurface surface)
    {
        if (texture == null || texture.IsDisposed)
        {
            return;
        }

        surface.Canvas.Clear(Colors.Transparent);
        surface.Canvas.Save();

        ScaleCanvas(surface.Canvas, texture, stretch, bounds);
        surface.Canvas.DrawSurface(texture.DrawingSurface, 0, 0);

        surface.Canvas.Restore();
    }

    private void ScaleCanvas(Canvas canvas, Texture texture, Stretch stretch, Rect bounds)
    {
        float x = (float)texture.Size.X;
        float y = (float)texture.Size.Y;

        if (stretch == Stretch.Fill)
        {
            canvas.Scale((float)bounds.Width / x, (float)bounds.Height / y);
        }
        else if (stretch == Stretch.Uniform)
        {
            float scaleX = (float)bounds.Width / x;
            float scaleY = (float)bounds.Height / y;
            var scale = Math.Min(scaleX, scaleY);
            float dX = (float)bounds.Width / 2 / scale - x / 2;
            float dY = (float)bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
        else if (stretch == Stretch.UniformToFill)
        {
            float scaleX = (float)bounds.Width / x;
            float scaleY = (float)bounds.Height / y;
            var scale = Math.Max(scaleX, scaleY);
            float dX = (float)bounds.Width / 2 / scale - x / 2;
            float dY = (float)bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
    }
}
