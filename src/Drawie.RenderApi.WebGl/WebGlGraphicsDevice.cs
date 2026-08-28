using System.Text;
using Drawie.Backend.Shaders.Common;
using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlGraphicsDevice : IGraphicsDevice
{
    public int GlHandle { get; }

    public WebGlGraphicsDevice(int gl)
    {
        GlHandle = gl;
    }

    public IBuffer<TData> CreateBuffer<TData>(BufferUsage usage, TData[]? data) where TData : unmanaged
    {
        return new WebGlBuffer<TData>(GlHandle, usage, data);
    }

    public ITexture CreateTexture(TextureDesc desc)
    {
        return new WebGlTexture(GlHandle, desc.Width, desc.Height);
    }

    public IPipeline CreatePipeline(PipelineDesc desc)
    {
        return new WebGlPipeline(desc, GlHandle);
    }

    public ICommandList CreateCommandList()
    {
        return new WebGlCommandList(GlHandle);
    }

    public ISampler CreateSampler(SamplerDesc desc)
    {
        return new WebGlSampler(GlHandle, desc);
    }

    public IShaderProgram CreateShaderProgram(ShaderProgramDesc desc)
    {
        WebGlShaderProgram webGlProgram = new WebGlShaderProgram(GlHandle);
        int program = webGlProgram.ProgramHandle;

        int[] shaders = new int[desc.Shaders.Count];

        try
        {
            for (int i = 0; i < desc.Shaders.Count; i++)
            {
                var shader = desc.Shaders[i];

                int shaderHandle = JSRuntime.CreateShader(
                    GlHandle,
                    ToWebGlShaderType(shader.Type));

                shaders[i] = shaderHandle;

                string source = Encoding.UTF8.GetString(shader.Bytes);

                JSRuntime.ShaderSource(GlHandle, shaderHandle, source);
                string? result = JSRuntime.CompileShader(GlHandle, shaderHandle);

                if(result != null)
                {
                    throw new Exception($"Shader compilation failed: {result}");
                }

                JSRuntime.AttachShader(GlHandle, program, shaderHandle);
            }

            string? error = JSRuntime.LinkProgram(GlHandle, program);
            if (error != null)
            {
                throw new Exception($"Program linking failed: {error}");
            }

            return webGlProgram;
        }
        finally
        {
            for (int i = 0; i < shaders.Length; i++)
            {
                int shader = shaders[i];

                if (shader == 0)
                    continue;

                // TODO:
                /*JSRuntime.DetachShader(
                    GlHandle,
                    program,
                    shader);

                JSRuntime.DeleteShader(
                    GlHandle,
                    shader);*/
            }
        }
    }

    private static int ToWebGlShaderType(ShaderType type)
    {
        return type switch
        {
            ShaderType.Vertex => (int)WebGlShaderType.Vertex,
            ShaderType.Fragment => (int)WebGlShaderType.Fragment,
            _ => throw new NotSupportedException(
                $"Unsupported WebGL shader type: {type}")
        };
    }

    public IRenderTarget CreateRenderTarget(TextureDesc textureDesc)
    {
        return new WebGlRenderTarget(GlHandle, textureDesc.Width, textureDesc.Height, textureDesc.Depth);
    }

    public IBufferGroup CreateBufferGroup()
    {
        return new WebGlVertexArray(GlHandle);
    }

    public void Submit(RecordedRenderPass cmdList)
    {
        for (var index = 0; index < cmdList.Instructions.Length; index++)
        {
            var instruction = cmdList.Instructions[index];
            instruction.Invoke();
        }
    }
}