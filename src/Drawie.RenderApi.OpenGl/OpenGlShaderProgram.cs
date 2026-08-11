using System.Runtime.InteropServices;
using Drawie.RenderApi.Abstraction.Shaders;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlShaderProgram : IShaderProgram
{
    public uint ProgramHandle { get; }
    public GL Api { get; }

    private Dictionary<string, uint> uniformBlockUbos = new Dictionary<string, uint>();

    public OpenGlShaderProgram(GL gl, uint programHandle)
    {
        ProgramHandle = programHandle;
        Api = gl;
    }

    public void Use()
    {
        Api.UseProgram(ProgramHandle);
    }

    public unsafe void UpdateUniforms(List<UniformBlock> uniformBlocks)
    {
        uint bindingPoint = 0;

        foreach (var uniformBlock in uniformBlocks)
        {
            int blockIndex = uniformBlock.ShaderLayout.Index;
            int blockSize = uniformBlock.ShaderLayout.Size;

            if (blockIndex == int.MaxValue)
                continue;

            Api.UniformBlockBinding(
                ProgramHandle,
                (uint)blockIndex,
                bindingPoint);

            if (!uniformBlockUbos.TryGetValue(
                    uniformBlock.Name,
                    out uint ubo))
            {
                ubo = Api.GenBuffer();
                uniformBlockUbos.Add(
                    uniformBlock.Name,
                    ubo);
            }

            Api.BindBuffer(
                BufferTargetARB.UniformBuffer,
                ubo);

            Api.BufferData(
                BufferTargetARB.UniformBuffer,
                (nuint)blockSize,
                null,
                BufferUsageARB.DynamicDraw);

            Api.BindBufferBase(
                BufferTargetARB.UniformBuffer,
                bindingPoint,
                ubo);

            // Upload individual properties.
            for (var i = 0; i < uniformBlock.Properties.Count; i++)
            {
                var property = uniformBlock.Properties[i];
                UploadProperty(property.ObjValue, uniformBlock.ShaderLayout.UniformProperties[i]);
            }

            Api.BindBuffer(
                BufferTargetARB.UniformBuffer,
                0);

            bindingPoint++;
        }
    }

    private unsafe void UploadProperty(
        object value,
        UniformPropertyLayout layout)
    {
        int size = layout.Size;

        GCHandle handle = GCHandle.Alloc(
            value,
            GCHandleType.Pinned);

        try
        {
            Api.BufferSubData(
                BufferTargetARB.UniformBuffer,
                layout.Offset,
                (nuint)size,
                (void*)handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }
}