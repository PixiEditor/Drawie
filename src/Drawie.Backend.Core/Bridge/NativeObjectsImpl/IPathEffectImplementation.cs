namespace Drawie.Backend.Core.Bridge.NativeObjectsImpl;

public interface IPathEffectImplementation
{
    public IntPtr CreateDash(float[] intervals, float phase);
    public void Dispose(IntPtr pathEffectPointer);
    public object GetNativePathEffect(IntPtr objectPointer);
}
