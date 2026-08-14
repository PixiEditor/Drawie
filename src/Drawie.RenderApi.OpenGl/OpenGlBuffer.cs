using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.OpenGL.Extensions;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlBuffer<TData> : IBuffer<TData> where TData : unmanaged
{
    public uint NativeHandle => openglHandle;
    public BufferUsage Usage { get; }
    public ulong Size { get; }

    private uint openglHandle;
    private GL api;


    public OpenGlBuffer(GL api, BufferUsage usage, TData[]? data = null)
    {
        Usage = usage;
        this.api = api;
        
        openglHandle = api.GenBuffer();
        if (data != null)
        {
            unsafe
            {
                Size = (uint)data.Length;
                fixed (void* d = data)
                {
                    BufferTargetARB bufferType = ToBufferType();
                    api.BindBuffer(bufferType, openglHandle);
                    api.BufferData(bufferType, (nuint)(Size * (uint)sizeof(TData)), d, BufferUsageARB.StaticDraw);
                }
            }
        }

        if (usage == BufferUsage.Vertex)
        {
            VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 8, 0);
            VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 8, 3);
            VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 8, 6);
        }
    }

    public void Dispose()
    {
        api.DeleteBuffer(openglHandle);
    }

    private BufferTargetARB ToBufferType()
    {
        return Usage.ToOpenGlTargetARB();
    }
    
    private unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, uint vertexSize,
        int offset)
    {
        int vTypeSize = sizeof(float);
        api.VertexAttribPointer(index, count, type, false, vertexSize * (uint) vTypeSize, (void*) (offset * vTypeSize));
        api.EnableVertexAttribArray(index);
    }
}