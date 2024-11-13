using System.Drawing;
using Drawie.Numerics;
using Drawie.RenderApi.OpenGL.Exceptions;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlWindowRenderApi : IOpenGlWindowRenderApi
{
    public event Action? FramebufferResized;
    public ITexture RenderTexture => texture;
    
    public IGLContext Context { get; private set; }

    private GL Api { get; set; }

    private uint vao;
    private uint vbo;
    private uint ebo;
    private uint program;
    private OpenGlTexture texture;

    static readonly float[] vertices =
    {
        //       aPosition     | aTexCoords
        1f, 1f, 0.0f, 1.0f, 1.0f, 1f, -1f, 0.0f, 1.0f, 0.0f, -1f, -1f, 0.0f, 0.0f, 0.0f, -1f, 1f, 0.0f, 0.0f, 1.0f
    };

    static readonly uint[] indices = { 0u, 1u, 3u, 1u, 2u, 3u };

    private const string vertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTextureCoord;

out vec2 frag_texCoords;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    frag_texCoords = aTextureCoord;
}";

    private const string fragmentShader = @"
#version 330 core

in vec2 frag_texCoords;

out vec4 out_color;

uniform sampler2D uTexture;

void main()
{
    out_color = texture(uTexture, frag_texCoords); 
}";

    public unsafe void CreateInstance(object contextObject, VecI framebufferSize)
    {
        if (contextObject is not IGLContext glContext)
            throw new ArgumentException("contextObject must be an INativeWindow");

        Context = glContext;
        Api = GL.GetApi(glContext);
        vao = Api.GenVertexArray();
        Api.BindVertexArray(vao);

        vbo = Api.GenBuffer();
        Api.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        SetVerticesBufferData();

        ebo = Api.GenBuffer();
        Api.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

        SetIndicesBufferData();

        var vertexShaderHandle = Api.CreateShader(ShaderType.VertexShader);
        Api.ShaderSource(vertexShaderHandle, vertexShader);

        Api.CompileShader(vertexShaderHandle);

        Api.GetShader(vertexShaderHandle, ShaderParameterName.CompileStatus, out var success);
        if (success == 0)
        {
            Api.GetShaderInfoLog(vertexShaderHandle, out var infoLog);
            throw new OpenGlException($"Vertex shader compilation failed: {infoLog}");
        }

        var fragmentShaderHandle = Api.CreateShader(ShaderType.FragmentShader);
        Api.ShaderSource(fragmentShaderHandle, fragmentShader);

        Api.CompileShader(fragmentShaderHandle);

        Api.GetShader(fragmentShaderHandle, ShaderParameterName.CompileStatus, out success);
        if (success == 0)
        {
            Api.GetShaderInfoLog(fragmentShaderHandle, out var infoLog);
            throw new OpenGlException($"Fragment shader compilation failed: {infoLog}");
        }

        program = Api.CreateProgram();
        Api.AttachShader(program, vertexShaderHandle);
        Api.AttachShader(program, fragmentShaderHandle);
        Api.LinkProgram(program);

        Api.GetProgram(program, GLEnum.LinkStatus, out success);
        if (success == 0)
        {
            Api.GetProgramInfoLog(program, out var infoLog);
            throw new OpenGlException($"Program linking failed: {infoLog}");
        }

        Api.DetachShader(program, vertexShaderHandle);
        Api.DetachShader(program, fragmentShaderHandle);
        Api.DeleteShader(vertexShaderHandle);
        Api.DeleteShader(fragmentShaderHandle);

        const uint positionLocation = 0;
        Api.EnableVertexAttribArray(positionLocation);
        Api.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);

        const uint texCoordLocation = 1;
        Api.EnableVertexAttribArray(texCoordLocation);
        Api.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
            (void*)(3 * sizeof(float)));

        Api.BindVertexArray(0);
        Api.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        Api.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        texture = new OpenGlTexture(Api, framebufferSize.X, framebufferSize.Y);
        texture.Activate(0);
        texture.Bind();

        int location = Api.GetUniformLocation(program, "uTexture");
        Api.Uniform1(location, 0);

        Api.Enable(EnableCap.Blend);
        Api.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public void DestroyInstance()
    {
        Api = null;
    }

    public void UpdateFramebufferSize(int width, int height)
    {
    }

    public void PrepareTextureToWrite()
    {
    }

    public unsafe void Render(double deltaTime)
    {
        Api.ClearColor(0, 0, 0, 1);
        Api.Clear((uint)ClearBufferMask.ColorBufferBit);

        Api.BindVertexArray(vao);
        Api.UseProgram(program);

        texture.Activate(0);
        texture.Bind();

        Api.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
    }

    private unsafe void SetVerticesBufferData()
    {
        fixed (float* verticesPtr = vertices)
        {
            Api.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), verticesPtr,
                BufferUsageARB.StaticDraw);
        }
    }

    private unsafe void SetIndicesBufferData()
    {
        fixed (uint* indicesPtr = indices)
        {
            Api.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), indicesPtr,
                BufferUsageARB.StaticDraw);
        }
    }
}
