using Silk.NET.OpenGL;

namespace SilkNet;

public sealed class BufferObject<TDataType> : NativeObject where TDataType : unmanaged
{
    private BufferTargetARB _bufferType;

    public unsafe BufferObject(GL glContext, Span<TDataType> data, BufferTargetARB bufferType) : base(glContext, glContext.GenBuffer())
    {
        _bufferType = bufferType;

        Bind();
        fixed (void* d = data)
        {
            glContext.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, GLEnum.StaticDraw);
        }
    }

    public void Bind()
    {
        GlContext.BindBuffer(_bufferType, Handle);
    }

    public override void Dispose()
    {
        GlContext.DeleteBuffer(Handle);
    }
}