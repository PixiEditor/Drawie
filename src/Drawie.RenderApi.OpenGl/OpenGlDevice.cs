using System.Numerics;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;
using ShaderType = Drawie.Backend.Shaders.Common.ShaderType;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlDevice : IGraphicsDevice
{
    private readonly IOpenGlContext context;
    private int handleCounter = 0;
    public GL Api { get; }

    public OpenGlDevice(IOpenGlContext graphicsContext)
    {
        Api = new GL(new LamdaNativeContext(graphicsContext.GetGlInterface));
        context = graphicsContext;
    }

    public IBuffer<TData> CreateBuffer<TData>(BufferUsage usage, TData[]? data) where TData : unmanaged
    {
        return new OpenGlBuffer<TData>(Api, usage, data);
    }

    public ITexture CreateTexture(TextureDesc desc)
    {
        var texture = new OpenGlTexture(Api, desc.Width, desc.Height, desc.Samples);
        context.AddManagedTexture(texture);
        return texture;
    }

    public IPipeline CreatePipeline(PipelineDesc desc)
    {
        return new OpenGlPipeline(desc, Api);
    }

    public ICommandList CreateCommandList()
    {
        return new OpenGlCommandList(Api);
    }

    public ISampler CreateSampler(SamplerDesc desc)
    {
        return new OpenGlSampler(Api);
    }

    public void Submit(RecordedRenderPass recordedRenderPass)
    {
        recordedRenderPass.Execute();
    }

    public unsafe IShaderProgram CreateShaderProgram(ShaderProgramDesc desc)
    {
        var program = Api.CreateProgram();

        uint[] shaders = new uint[desc.Shaders.Count];

        for (var i = 0; i < desc.Shaders.Count; i++)
        {
            var shader = desc.Shaders[i];
            shaders[i] = Api.CreateShader(
                ToOpenGlShaderType(shader.Type));

            uint shaderPtr = shaders[i];
            fixed (byte* bytes = shader.Bytes)
            {
                Api.ShaderBinary(
                    1,
                    &shaderPtr,
                    ShaderBinaryFormat.ShaderBinaryFormatSpirV,
                    bytes,
                    (uint)shader.Bytes.Length);
            }

            Api.SpecializeShader(
                shaderPtr,
                shader.EntryName,
                0,
                null,
                null);

            Api.AttachShader(program, shaderPtr);
        }

        Api.LinkProgram(program);
        
        Api.GetProgram(program, GLEnum.LinkStatus, out var status);

        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {Api.GetProgramInfoLog(program)}");
        }

        for (var i = 0; i < shaders.Length; i++)
        {
            if (shaders[i] != 0)
            {
                Api.DetachShader(program, shaders[i]);
                Api.DeleteShader(shaders[i]);
            }
        }

        return new OpenGlShaderProgram(Api, program);
    }


    public IRenderTarget CreateRenderTarget(TextureDesc textureDesc)
    {
        return new OpenGlRenderTarget(Api, textureDesc);
    }

    public IBufferGroup CreateBufferGroup()
    {
        return new OpenGlVertexArrayObject(Api);
    }


    private Silk.NET.OpenGL.ShaderType ToOpenGlShaderType(ShaderType shaderType)
    {
        switch (shaderType)
        {
            case ShaderType.Vertex:
                return Silk.NET.OpenGL.ShaderType.VertexShader;
            case ShaderType.Fragment:
                return Silk.NET.OpenGL.ShaderType.FragmentShader;
            default:
                throw new ArgumentOutOfRangeException(nameof(shaderType), shaderType, null);
        }
    }

    public void Dispose()
    {
        
    }
}