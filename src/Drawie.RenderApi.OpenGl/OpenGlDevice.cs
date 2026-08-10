using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;
using ShaderType = Drawie.RenderApi.Abstraction.Shaders.ShaderType;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlDevice : IGraphicsDevice
{
    private int handleCounter = 0;
    public GL Api { get; }

    public OpenGlDevice(GL api)
    {
        Api = api;
    }

    public Dictionary<int, INativeObject> ManagedObjects { get; } = new Dictionary<int, INativeObject>();

    public IBuffer CreateBuffer<TData>(BufferUsage usage, TData[]? data) where TData : unmanaged
    {
        return new OpenGlBuffer<TData>(Api, GetNextHandle(), usage, data);
    }

    public ITexture CreateTexture(TextureDesc desc)
    {
        throw new NotImplementedException();
    }

    public IPipeline CreatePipeline(PipelineDesc desc)
    {
        return new OpenGlPipeline(desc, Api);
    }

    public ICommandList CreateCommandList()
    {
        return new OpenGlCommandList(Api);
    }

    public void Submit(RecordedRenderPass recordedRenderPass)
    {
        foreach (var instruction in recordedRenderPass.Instructions)
        {
            instruction.Invoke();
        }
    }

    public unsafe IShaderProgram CreateShaderProgram(ShaderProgramDesc desc)
    {
        var program = Api.CreateProgram();

        uint vertex = 0;
        uint fragment = 0;

        if (desc.VertexShaderBytes != null)
        {
            vertex = Api.CreateShader(
                ToOpenGlShaderType(ShaderType.Vertex));
            
            fixed (byte* bytes = desc.VertexShaderBytes)
            {
                Api.ShaderBinary(
                    1,
                    &vertex,
                    ShaderBinaryFormat.ShaderBinaryFormatSpirV,
                    bytes,
                    (uint)desc.VertexShaderBytes.Length);
            }

            Api.SpecializeShader(
                vertex,
                "VSMain",
                0,
                null,
                null);

            Api.AttachShader(program, vertex);
        }

        if (desc.FragmentShaderBytes != null)
        {
            fragment = Api.CreateShader(
                ToOpenGlShaderType(ShaderType.Fragment));
            
            fixed (byte* bytes = desc.FragmentShaderBytes)
            {
                Api.ShaderBinary(
                    1,
                    &fragment,
                    ShaderBinaryFormat.ShaderBinaryFormatSpirV,
                    bytes,
                    (uint)desc.FragmentShaderBytes.Length);
            }

            Api.SpecializeShader(
                fragment,
                "FSMain",
                0,
                null,
                null);

            Api.AttachShader(program, fragment);
        }
        
        Api.LinkProgram(program);
        
        Api.GetProgram(program, GLEnum.LinkStatus, out var status);

        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {Api.GetProgramInfoLog(program)}");
        }
        
        if (vertex != 0)
        {
            Api.DetachShader(program, vertex);
            Api.DeleteShader(vertex);
        }

        if (fragment != 0)
        {
            Api.DetachShader(program, fragment);
            Api.DeleteShader(fragment);
        }

        return new OpenGlShaderProgram(Api, program);
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

    private int GetNextHandle()
    {
        handleCounter++;
        return handleCounter;
    }
}