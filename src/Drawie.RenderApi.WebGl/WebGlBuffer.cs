using System.ComponentModel;
using System.Runtime.InteropServices;
using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlBuffer<TData> : IBuffer<TData> where TData : unmanaged
{
    public int Id { get; }
    public int Gl { get; }
    public BufferUsage Usage { get; }
    public uint Size { get; }

    public WebGlBuffer(int gl, BufferUsage usage, TData[]? data = null)
    {
        Usage = usage;
        Gl = gl;

        Id = JSRuntime.CreateBuffer(gl);
        if (data != null)
        {
            Size = (uint)data.Length;
            WebGlBufferType bufferType = ToBufferType();
            JSRuntime.BindBuffer(Gl, (int)bufferType, Id);
            var bytes = MemoryMarshal.AsBytes(data).ToArray();
            JSRuntime.BufferData(Gl, (int)bufferType, bytes, (int)WebGlBufferUsage.StaticDraw);
        }

        if (usage == BufferUsage.Vertex)
        {
            VertexAttributePointer(0, 3, 8, 0);
            VertexAttributePointer(1, 3, 8, 3);
            VertexAttributePointer(2, 2, 8, 6);
        }
    }

    private WebGlBufferType ToBufferType()
    {
        return Usage switch
        {
            BufferUsage.Vertex => WebGlBufferType.Array,
            BufferUsage.Index => WebGlBufferType.ElementArray,
            BufferUsage.Uniform => WebGlBufferType.Uniform,
            BufferUsage.Storage =>
                throw new InvalidEnumArgumentException("Storage buffers are not supported in WebGL."),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void VertexAttributePointer(int index, int count, int vertexSize,
        int offset)
    {
        int vTypeSize = sizeof(float);
        JSRuntime.VertexAttribPointer(Gl, index, count, (int)WebGlDataType.Float, false, vertexSize * vTypeSize,
            (offset * vTypeSize));
        JSRuntime.EnableVertexAttribArray(Gl, index);
    }

    public void Dispose()
    {
        //TODO:
        //JSRuntime.DeleteBuffer(Gl, Id);
    }
}