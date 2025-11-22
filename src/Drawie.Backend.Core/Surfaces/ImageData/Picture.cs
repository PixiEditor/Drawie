using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Surfaces.ImageData;

public class Picture : NativeObject
{
    public override object Native => DrawingBackendApi.Current.PictureImplementation.GetNativePicture(ObjectPointer);
    
    public RectD CullRect => DrawingBackendApi.Current.PictureImplementation.GetCullRect(this);
    
    public Picture(IntPtr objPtr) : base(objPtr)
    {
    }
    
    public override void Dispose()
    {
        DrawingBackendApi.Current.PictureImplementation.DisposePicture(this);
    }

    public Shader? ToShader(TileMode tileModeX, TileMode tileModeY, FilterMode filterMode, Matrix3X3 localMatrix, RectD tile)
    {
        return DrawingBackendApi.Current.PictureImplementation.ToShader(this, tileModeX, tileModeY, filterMode, localMatrix, tile);
    }

    public void Serialize(System.IO.Stream stream)
    {
        DrawingBackendApi.Current.PictureImplementation.Serialize(this, stream);
    }
}
