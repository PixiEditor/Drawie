using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Shaders;

public class Blender : NativeObject
{
    public override object Native => DrawingBackendApi.Current.BlenderImplementation.GetNativeObject(ObjectPointer);

    public Blender(IntPtr objPtr) : base(objPtr)
    {
    }

    public static Blender? CreateFromString(string blenderCode, out string? errors)
    {
        var objPtr = DrawingBackendApi.Current.BlenderImplementation.CreateFromString(blenderCode, out errors);
        if (objPtr == IntPtr.Zero)
        {
            return null;
        }

        return new Blender(objPtr);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.BlenderImplementation.Dispose(ObjectPointer);
    }
}
