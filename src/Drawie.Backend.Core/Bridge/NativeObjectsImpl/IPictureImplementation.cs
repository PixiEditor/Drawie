using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IPictureImplementation
{
    object GetNativePicture(IntPtr objectPointer);
    void DisposePicture(Picture picture);
    RectD GetCullRect(Picture picture);

    Shader? ToShader(Picture picture, TileMode tileModeX, TileMode tileModeY, FilterMode filterMode, Matrix3X3 localMatrix, RectD tile);
    public void Serialize(Picture picture, System.IO.Stream stream);
}
