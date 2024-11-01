using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi.Html5Canvas;
using Drawie.RenderApi.WebGl.Enums;
using Drawie.RenderApi.WebGl.Exceptions;

namespace Drawie.RenderApi.WebGl;

public class WebGlWindowRenderApi : IWindowRenderApi
{
    private const string vertexSource = """
                                            attribute vec4 position;
                                            void main() {
                                                gl_Position = position;
                                            }
                                        """;

    private const string fragSource = """
                                          void main() {
                                              gl_FragColor = vec4(1.0, 0.5, 0.0, 1.0);
                                          }
                                      """;

    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;
    public ITexture RenderTexture => canvasObject;

    public string CanvasId { get; private set; }
    
    private int posBuffer;
    private int program;
    public int gl;
    
    private int vertexPosAttrib;

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());

        gl = JSRuntime.OpenCanvasContext(CanvasId, "webgl");

        var vertexShader = LoadShader(gl, vertexSource, WebGlShaderType.Vertex);
        var fragmentShader = LoadShader(gl, fragSource, WebGlShaderType.Fragment);

        program = InitProgram(gl, vertexShader, fragmentShader);

        posBuffer = InitBuffers(gl);
        
        vertexPosAttrib = JSRuntime.GetAttribLocation(gl, program, "position");
        
        Render(0);
    }

    public void DestroyInstance()
    {
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        canvasObject.SetAttribute("width", width.ToString());
        canvasObject.SetAttribute("height", height.ToString());
        FramebufferResized?.Invoke();
    }

    public void PrepareTextureToWrite()
    {
    }

    public void Render(double deltaTime)
    {
        JSRuntime.ClearColor(gl, 0.0f, 0.0f, 0.0f, 1.0f);
        JSRuntime.Clear(gl, (int)WebGlBufferMask.ColorBufferBit);

        JSRuntime.BindBuffer(gl, (int)WebGlBufferType.Array, posBuffer);
        JSRuntime.VertexAttribPointer(gl, vertexPosAttrib, 2, (int)WebGlArrayType.Float, false, 0, 0);
        JSRuntime.EnableVertexAttribArray(gl, vertexPosAttrib);
        
        JSRuntime.UseProgram(gl, program);
        JSRuntime.DrawArrays(gl, (int)WebGlDrawMode.TriangleStrip, 0, 4);
    }

    private int LoadShader(int handle, string shader, WebGlShaderType type)
    {
        int shaderHandle = JSRuntime.CreateShader(handle, (int)type);
        JSRuntime.ShaderSource(handle, shaderHandle, shader);
        string? error = JSRuntime.CompileShader(handle, shaderHandle);

        if (error != null)
        {
            throw new ShaderCompilationException(type, shader, error);
        }

        return shaderHandle;
    }

    private int InitProgram(int handle, int vertexShader, int fragmentShader)
    {
        int program = JSRuntime.CreateProgram(handle);
        JSRuntime.AttachShader(handle, program, vertexShader);
        JSRuntime.AttachShader(handle, program, fragmentShader);
        string? error = JSRuntime.LinkProgram(handle, program);

        if (error != null)
        {
            throw new WebGlException(error);
        }

        return program;
    }

    private int InitBuffers(int handle)
    {
        int positionBuffer = JSRuntime.CreateBuffer(handle);
        JSRuntime.BindBuffer(handle, (int)WebGlBufferType.Array, positionBuffer);
        double[] vertices = new double[] { 1.0f, 1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, -1.0f };

        JSRuntime.BufferData(handle, (int)WebGlBufferType.Array, vertices, (int)WebGlBufferUsage.StaticDraw);

        return positionBuffer;
    }
}
