using Avalonia;
using Avalonia.Media;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace Drawie.Interop.Avalonia.Core.Controls;

public class DrawieTextureControl : DrawieControl
{
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<DrawieTextureControl, Stretch>(
            nameof(Stretch), Stretch.Uniform);

    public static readonly StyledProperty<bool> RepaintOnChangedProperty = AvaloniaProperty.Register<DrawieTextureControl, bool>(
        nameof(RepaintOnChanged));

    public bool RepaintOnChanged
    {
        get => GetValue(RepaintOnChangedProperty);
        set => SetValue(RepaintOnChangedProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    public static readonly StyledProperty<NativeTexture> TextureProperty =
        AvaloniaProperty.Register<DrawieTextureControl, NativeTexture>(
            nameof(NativeTexture));

    public static readonly StyledProperty<SampleQuality> SamplingOptionsProperty =
        AvaloniaProperty.Register<DrawieTextureControl, SampleQuality>(
            nameof(SamplingOptions));

    public SampleQuality SamplingOptions
    {
        get => GetValue(SamplingOptionsProperty);
        set => SetValue(SamplingOptionsProperty, value);
    }

    public NativeTexture NativeTexture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    static DrawieTextureControl()
    {
        AffectsMeasure<DrawieTextureControl>(TextureProperty, StretchProperty);
        TextureProperty.Changed.AddClassHandler<DrawieTextureControl>((x,e) =>
        {
            x.QueueNextFrame();
            if (e.OldValue is NativeTexture oldTexture && x.RepaintOnChanged)
                oldTexture.Changed -= x.NativeTextureChanged;
            if (e.NewValue is NativeTexture newTexture && x.RepaintOnChanged)
                newTexture.Changed += x.NativeTextureChanged;
        });
        SamplingOptionsProperty.Changed.AddClassHandler<DrawieTextureControl>((x,e) => x.QueueNextFrame());
        StretchProperty.Changed.AddClassHandler<DrawieTextureControl>((x,e) => x.QueueNextFrame());
        RepaintOnChangedProperty.Changed.AddClassHandler<DrawieTextureControl>((x,e) =>
        {
            if (e.NewValue is true)
            {
                x.QueueNextFrame();
                if(x.NativeTexture != null)
                    x.NativeTexture.Changed += x.NativeTextureChanged;
            }
            else
            {
                if(x.NativeTexture != null)
                    x.NativeTexture.Changed -= x.NativeTextureChanged;
            }
        });
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var source = NativeTexture;
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
        var source = NativeTexture;

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

    private void NativeTextureChanged(RectD? changedRect)
    {
        QueueNextFrame();
    }

    public override void Draw(DrawingSurface surface)
    {
        if (NativeTexture == null || NativeTexture.IsDisposed)
        {
            return;
        }

        surface.Canvas.Clear(Colors.Transparent);
        surface.Canvas.Save();

        ScaleCanvas(surface.Canvas);
        if (SamplingOptions == SampleQuality.Nearest)
        {
            surface.Canvas.DrawSurface(NativeTexture.DrawingSurface, 0, 0);
        }
        else
        {
            using var snapshot = NativeTexture.DrawingSurface.Snapshot();
            surface.Canvas.DrawImage(snapshot, 0, 0, Backend.Core.Surfaces.SamplingOptions.Bilinear);
        }

        surface.Canvas.Restore();
    }

    private void ScaleCanvas(Canvas canvas)
    {
        float x = (float)NativeTexture.Size.X;
        float y = (float)NativeTexture.Size.Y;

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

public enum SampleQuality
{
    Nearest,
    Bilinear,
}
