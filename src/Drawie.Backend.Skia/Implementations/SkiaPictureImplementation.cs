using Drawie.Backend.Core.Bridge.NativeObjectsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using Drawie.Skia.Extensions;
using SkiaSharp;

namespace Drawie.Skia.Implementations;

public class SkiaPictureImplementation : SkObjectImplementation<SKPicture>, IPictureImplementation
{
    private SkiaShaderImplementation _shaderImplementation;

    public SkiaPictureImplementation(SkiaShaderImplementation shaderImplementation)
    {
        _shaderImplementation = shaderImplementation;
    }

    public object GetNativePicture(IntPtr objectPointer)
    {
        if (TryGetInstance(objectPointer, out var picture))
        {
            return picture ?? throw new InvalidOperationException("SKPicture instance is null.");
        }

        throw new InvalidOperationException("No SKPicture found for the given pointer.");
    }

    public void DisposePicture(Picture picture)
    {
        UnmanageAndDispose(picture.ObjectPointer);
    }

    public RectD GetCullRect(Picture picture)
    {
        var skPicture = (SKPicture)GetNativePicture(picture.ObjectPointer);
        var rect = skPicture.CullRect;
        return new RectD(rect.Left, rect.Top, rect.Width, rect.Height);
    }

    public Shader? ToShader(Picture picture, TileMode tileModeX, TileMode tileModeY, FilterMode filterMode,
        Matrix3X3 localMatrix, RectD tile)
    {
        if (!TryGetInstance(picture.ObjectPointer, out var skPicture) || skPicture is null)
        {
            return null;
        }

        var shader = skPicture.ToShader((SKShaderTileMode)tileModeX, (SKShaderTileMode)tileModeY,
            (SKFilterMode)filterMode, localMatrix.ToSkMatrix(), tile.ToSkRect());
        _shaderImplementation.AddManagedInstance(shader);

        return new Shader(shader.Handle);
    }
}
