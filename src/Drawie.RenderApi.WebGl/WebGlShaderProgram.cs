using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.Backend.Shaders.Common;
using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlShaderProgram : IShaderProgram
{
    public int ProgramHandle { get; }
    public int Gl { get; }

    private readonly Dictionary<string, int> uniformBlockUbos = new();

    public WebGlShaderProgram(int gl)
    {
        Gl = gl;
        ProgramHandle = JSRuntime.CreateProgram(gl);
    }

    public void Use()
    {
        JSRuntime.UseProgram(Gl, ProgramHandle);
    }

    public void UpdateUniforms(List<UniformBlock> blocks)
    {
        int bindingPoint = 0;

        foreach (var uniformBlock in blocks)
        {
            int blockIndex = uniformBlock.ShaderLayout.Index;
            int blockSize = uniformBlock.ShaderLayout.Size;

            if (blockIndex == int.MaxValue)
                continue;

            JSRuntime.UniformBlockBinding(
                Gl,
                ProgramHandle,
                blockIndex,
                bindingPoint);

            if (!uniformBlockUbos.TryGetValue(
                    uniformBlock.Name,
                    out int ubo))
            {
                ubo = JSRuntime.CreateBuffer(Gl);

                uniformBlockUbos.Add(
                    uniformBlock.Name,
                    ubo);
            }

            JSRuntime.BindBuffer(
                Gl,
                (int)WebGlBufferType.Uniform,
                ubo);

            JSRuntime.BufferData(
                Gl,
                (int)WebGlBufferType.Uniform,
                blockSize,
                (int)WebGlBufferUsage.DynamicDraw);

            JSRuntime.BindBufferBase(
                Gl,
                (int)WebGlBufferType.Uniform,
                bindingPoint,
                ubo);

            for (int i = 0; i < uniformBlock.Properties.Count; i++)
            {
                var property = uniformBlock.Properties[i];

                UploadProperty(
                    property.ObjValue,
                    uniformBlock.ShaderLayout.UniformProperties[i]);
            }

            JSRuntime.BindBuffer(
                Gl,
                (int)WebGlBufferType.Uniform,
                0);

            bindingPoint++;
        }
    }

    private void UploadProperty(
        object value,
        PropertyLayout layout)
    {
        byte[] data = UniformValueToBytes(value);

        JSRuntime.BufferSubData(
            Gl,
            (int)WebGlBufferType.Uniform,
            layout.Offset,
            data);
    }

    private static byte[] UniformValueToBytes(object value)
    {
        return value switch
        {
            float v => BitConverter.GetBytes(v),
            int v => BitConverter.GetBytes(v),
            uint v => BitConverter.GetBytes(v),
            float[] v => MemoryMarshal.Cast<float, byte>(v.AsSpan()).ToArray(),
            int[] v => MemoryMarshal.Cast<int, byte>(v.AsSpan()).ToArray(),
            uint[] v => MemoryMarshal.Cast<uint, byte>(v.AsSpan()).ToArray(),
            Matrix4x4 v => CreateFromMatrix(v),
            _ => throw new NotSupportedException($"Unsupported uniform value type: {value.GetType()}")
        };
    }

    private static byte[] CreateFromMatrix(Matrix4x4 matrix)
    {
        return MemoryMarshal
            .AsBytes(MemoryMarshal.CreateSpan(ref matrix, 1))
            .ToArray();
    }
}