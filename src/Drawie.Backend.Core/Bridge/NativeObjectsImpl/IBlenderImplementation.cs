using Drawie.Backend.Core.Shaders;

namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IBlenderImplementation
{
    public IntPtr CreateFromString(string blenderCode, out string? errors);
    object GetNativeObject(IntPtr objectPointer);
    void Dispose(IntPtr objectPointer);
}
