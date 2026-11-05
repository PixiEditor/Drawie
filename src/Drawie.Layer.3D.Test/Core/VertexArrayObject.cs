using Silk.NET.OpenGL;

namespace SilkNet;

public sealed class VertexArrayObject<TVertexType, TIndexType> : NativeObject 
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    
    public VertexArrayObject(GL glContext, BufferObject<TVertexType> vbo, BufferObject<TIndexType> ebo) : base(glContext, glContext.GenVertexArray())
    {
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
        int offset)
    {
        int vTypeSize = sizeof(TVertexType);
        GlContext.VertexAttribPointer(index, count, type, false, vertexSize * (uint) vTypeSize, (void*) (offset * vTypeSize));
        GlContext.EnableVertexAttribArray(index);
    }
    
    public void Bind()
    {
        GlContext.BindVertexArray(Handle);
    }

    public override void Dispose()
    {
        GlContext.DeleteVertexArray(Handle);
    }
}