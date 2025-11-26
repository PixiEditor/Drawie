using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class TexturePaintable : Paintable
{
    public Texture Image { get; private set; }
    public override bool AnythingVisible => Image is { Size: {X: > 0, Y: > 0 }};

    private Image lastSnapshot;

    private bool disposeAfterUse;

    public TexturePaintable(Texture image)
    {
        Image = image;
        Image.LockDispose(this);
    }

    public TexturePaintable(Texture image, bool disposeAfterUse)
    {
        Image = image;
        Image.LockDispose(this);
        this.disposeAfterUse = disposeAfterUse;
        IsOneTimeUse = disposeAfterUse;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        if(Image is null || Image.Size.X <= 0 || Image.Size.Y <= 0 || Image.IsDisposed)
        {
            return null;
        }

        Matrix3X3 scalingMatrix = Matrix3X3.CreateScaleTranslation(
            (float)bounds.Width / Image.Size.X,
            (float)bounds.Height / Image.Size.Y,
            (float)bounds.X,
            (float)bounds.Y);

        lastSnapshot = Image.DrawingSurface.Snapshot();

        // TODO: Figure out if we need to change bilinear in any case
        var shader = lastSnapshot.ToShader(TileMode.Clamp, TileMode.Clamp, SamplingOptions.Bilinear, scalingMatrix.PostConcat(matrix));
        lastSnapshot?.Dispose();

        return shader;
    }

    public override void DisposeShaderElements()
    {
        // Remove disposal from GetShader and uncomment in case something breaks
        //lastSnapshot?.Dispose();
    }

    public override Paintable? Clone()
    {
        return new TexturePaintable(Image);
    }

    public override void ApplyOpacity(double opacity)
    {
        Texture surf = new Texture(Image.Size);
        using Paint paint = new Paint
        {
            Color = Colors.White.WithAlpha((byte)(opacity * 255)),
        };

        surf.DrawingSurface.Canvas.DrawSurface(Image.DrawingSurface, 0, 0, paint);
        Image = surf;
    }

    protected bool Equals(TexturePaintable other)
    {
        return Image.Equals(other.Image);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((TexturePaintable)obj);
    }

    public override int GetHashCode()
    {
        return Image.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Image}";
    }

    public override void Dispose()
    {
        base.Dispose();
        Image?.UnlockDispose(this);
        lastSnapshot?.Dispose();
        lastSnapshot = null;

        if (disposeAfterUse)
        {
            Image?.Dispose();
        }
    }
}
