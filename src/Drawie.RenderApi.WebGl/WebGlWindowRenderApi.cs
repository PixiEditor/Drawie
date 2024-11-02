using Drawie.JSInterop;
using Drawie.Numerics;
using Drawie.RenderApi.Html5Canvas;
using Drawie.RenderApi.WebGl.Enums;
using Drawie.RenderApi.WebGl.Exceptions;

namespace Drawie.RenderApi.WebGl;

public class WebGlWindowRenderApi : IWindowRenderApi
{
    private const string vertexSource = """
                                        #version 300 es
                                            in vec4 position;
                                            in vec2 aTextureCoord;
                                            
                                            out highp vec2 vTextureCoord;
                                            void main() {
                                                gl_Position = position;
                                                vTextureCoord = aTextureCoord;
                                            }
                                        """;

    private const string fragSource = """
                                      #version 300 es
                                        precision highp float;
                                        in highp vec2 vTextureCoord;
                                      
                                         uniform sampler2D uSampler;
                                         out vec4 fragColor;
                                      
                                         void main(void) {
                                           fragColor = texture(uSampler, vTextureCoord);
                                         }
                                      """;

    private HtmlCanvas canvasObject;
    public event Action? FramebufferResized;
    public ITexture RenderTexture => texture;

    public string CanvasId { get; private set; }
    
    private int posBuffer;
    private int program;
    public int gl;

    private WebGlTexture texture;
    
    private int vertexPosAttrib;
    private int texCoordAttrib;
    private int uSamplerUniform;

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        JSRuntime.InterceptGLObject();
        canvasObject = JSRuntime.CreateElement<HtmlCanvas>();
        CanvasId = canvasObject.Id;
        canvasObject.SetAttribute("width", framebufferSize.X.ToString());
        canvasObject.SetAttribute("height", framebufferSize.Y.ToString());

        gl = JSRuntime.OpenSkiaContext(canvasObject.Id);
        
        JSRuntime.MakeContextCurrent(gl);

        var vertexShader = LoadShader(gl, vertexSource, WebGlShaderType.Vertex);
        var fragmentShader = LoadShader(gl, fragSource, WebGlShaderType.Fragment);
        
        program = InitProgram(gl, vertexShader, fragmentShader);
        
        posBuffer = InitBuffers(gl);
        CreateTexture(gl, framebufferSize.X, framebufferSize.Y);
        InitTextureBuffer(gl);
        
        vertexPosAttrib = JSRuntime.GetAttribLocation(gl, program, "position");
        texCoordAttrib = JSRuntime.GetAttribLocation(gl, program, "aTextureCoord");
        uSamplerUniform = JSRuntime.GetUniformLocation(gl, program, "uSampler");
    }

    public void DestroyInstance()
    {
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        canvasObject.SetAttribute("width", width.ToString());
        canvasObject.SetAttribute("height", height.ToString());
        DisposeTexture();
        CreateTexture(gl, width, height);
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
        
        SetTextureData();
        
        JSRuntime.UseProgram(gl, program);
        
        JSRuntime.ActiveTexture(gl, 0);
        JSRuntime.BindTexture(gl, (int)WebGlTextureType.Texture2D, texture.TextureId);
        JSRuntime.Uniform1i(gl, uSamplerUniform, 0);
        
        JSRuntime.DrawArrays(gl, (int)WebGlDrawMode.TriangleStrip, 0, 4);
    }

    private int LoadShader(int handle, string shader, WebGlShaderType type)
    {
        int shaderHandle = JSRuntime.CreateShader(handle, (int)type);
        JSRuntime.ShaderSource(handle, shaderHandle, shader);
        string? error = JSRuntime.CompileShader(handle, shaderHandle);

        if (error != null)
        {
            Console.WriteLine(error);
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
    
    private void CreateTexture(int handle, int width, int height)
    {
        texture = new WebGlTexture(gl, JSRuntime.CreateTexture(handle));
        JSRuntime.BindTexture(handle, (int)WebGlTextureType.Texture2D, texture.TextureId);
        JSRuntime.TexImage2D(handle, (int)WebGlTextureType.Texture2D, 0, (int)WebGlTextureFormat.Rgba, width, height, 0, (int)WebGlTextureFormat.Rgba, (int)WebGlArrayType.UnsignedByte, 0);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMinFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMagFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapS, (int)WebGlTextureWrap.ClampToEdge);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapT, (int)WebGlTextureWrap.ClampToEdge);
    }
    
    private void DisposeTexture()
    {
        texture?.Dispose();
        texture = null;
    }
    
    private void InitTextureBuffer(int handle)
    {
        int texCoordBuffer = JSRuntime.CreateBuffer(handle);
        JSRuntime.BindBuffer(handle, (int)WebGlBufferType.Array, texCoordBuffer);
        double[] texCoords = new double[] { 1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f };
        JSRuntime.BufferData(handle, (int)WebGlBufferType.Array, texCoords, (int)WebGlBufferUsage.StaticDraw);
    }
    
    private void SetTextureData()
    {
        JSRuntime.BindBuffer(gl, (int)WebGlBufferType.Array, texCoordAttrib);
        JSRuntime.VertexAttribPointer(gl, texCoordAttrib, 2, (int)WebGlArrayType.Float, false, 0, 0);
        JSRuntime.EnableVertexAttribArray(gl, texCoordAttrib);
    }
}
