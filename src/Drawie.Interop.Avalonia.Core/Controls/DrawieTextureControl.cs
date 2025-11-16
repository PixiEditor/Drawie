using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;
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

    private Texture texture;
    private IExportedTexture next;
    private IExportedTexture? lastExportedTexture;

    static DrawieTextureControl()
    {
        AffectsRender<DrawieTextureControl>(TextureProperty, StretchProperty);
        AffectsMeasure<DrawieTextureControl>(TextureProperty, StretchProperty);
        TextureProperty.Changed.AddClassHandler<DrawieTextureControl>(OnTextureChanged);
    }

    private static void OnTextureChanged(DrawieTextureControl drawieTextureControl, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Texture newTexture)
        {
            drawieTextureControl.texture = newTexture;
        }
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

    public override void Draw(DrawingSurface surface)
    {
        if (Texture == null || Texture.IsDisposed)
        {
            return;
        }

        surface.Canvas.Clear(Colors.Transparent);
        surface.Canvas.Save();

        ScaleCanvas(surface.Canvas);
        surface.Canvas.DrawSurface(Texture.DrawingSurface, 0, 0);

        surface.Canvas.Restore();
    }

    public void UpdateTexture()
    {
        if (texture == null || texture.IsDisposed)
        {
            return;
        }

        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            if (next != null)
            {
                next.Dispose();
                next = null;
            }

            next = DrawingBackendApi.Current.CreateTextureToExport(texture);
        });
    }

    public override IExportedTexture GetTexture()
    {
        if (next != null)
        {
            lastExportedTexture = next;
            next = null;
        }

        return lastExportedTexture;
    }

    private void ScaleCanvas(Canvas canvas)
    {
        float x = (float)Texture.Size.X;
        float y = (float)Texture.Size.Y;

        if (Stretch == Stretch.Fill)
        {
            canvas.Scale((float)Bounds.Width / x, (float)Bounds.Height / y);
        }
        else if (Stretch == Stretch.Uniform)
        {
            float scaleX = (float)Bounds.Width / x;
            float scaleY = (float)Bounds.Height / y;
            var scale = Math.Min(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - x / 2;
            float dY = (float)Bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
        else if (Stretch == Stretch.UniformToFill)
        {
            float scaleX = (float)Bounds.Width / x;
            float scaleY = (float)Bounds.Height / y;
            var scale = Math.Max(scaleX, scaleY);
            float dX = (float)Bounds.Width / 2 / scale - x / 2;
            float dY = (float)Bounds.Height / 2 / scale - y / 2;
            canvas.Scale(scale, scale);
            canvas.Translate(dX, dY);
        }
    }
}
