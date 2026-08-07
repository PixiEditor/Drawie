using Silk.NET.OpenGL;

namespace SilkNet;

public abstract class NativeObject : IDisposable 
{
    public uint Handle { get; }
    protected GL GlContext { get;  }

    public NativeObject(GL glContext, uint handle)
    {
        GlContext = glContext;
        Handle = handle;
    }

    public abstract void Dispose();
}