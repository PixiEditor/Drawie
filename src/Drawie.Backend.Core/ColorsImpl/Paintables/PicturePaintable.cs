using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public class PicturePaintable : Paintable
{
    public Picture Picture { get; private set; }
    public override bool AnythingVisible => Picture != null;

    private bool disposeAfterUse;

    public PicturePaintable(Picture picture)
    {
        Picture = picture;
    }

    public PicturePaintable(Picture picture, bool disposeAfterUse)
    {
        Picture = picture;
        this.disposeAfterUse = disposeAfterUse;
        IsOneTimeUse = disposeAfterUse;
    }

    public override Shader? GetShader(RectD bounds, Matrix3X3 matrix)
    {
        if(Picture is null)
        {
            return null;
        }

        Matrix3X3 scalingMatrix = Matrix3X3.CreateScaleTranslation(
            (float)bounds.Width / (float)Picture.CullRect.Width,
            (float)bounds.Height / (float)Picture.CullRect.Height,
            (float)bounds.X,
            (float)bounds.Y);


        var shader = Picture.ToShader(TileMode.Clamp, TileMode.Clamp, FilterMode.Nearest, scalingMatrix.PostConcat(matrix),
            new RectD(0, 0, Picture.CullRect.Width, Picture.CullRect.Height));

        return shader;
    }

    public override void DisposeShaderElements()
    {
        
    }

    public override Paintable? Clone()
    {
        return new PicturePaintable(Picture);
    }

    public override void ApplyOpacity(double opacity)
    {
       // TODO: Implement
    }

    protected bool Equals(PicturePaintable other)
    {
        return Picture.Equals(other.Picture);
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

        return Equals((PicturePaintable)obj);
    }

    public override int GetHashCode()
    {
        return Picture.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Picture}";
    }

    public override void Dispose()
    {
        base.Dispose();
        if (disposeAfterUse)
        {
            Picture?.Dispose();
        }
    }
}
