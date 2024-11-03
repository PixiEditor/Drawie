using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    [JSImport("webgl.createShader", "drawie.js")]
    public static partial int CreateShader(int contextHandle, int shaderType);

    [JSImport("webgl.shaderSource", "drawie.js")]
    public static partial void ShaderSource(int handle, int shaderHandle, string shader);

    [JSImport("webgl.compileShader", "drawie.js")]
    public static partial string? CompileShader(int handle, int shaderHandle);

    [JSImport("webgl.createProgram", "drawie.js")]
    public static partial int CreateProgram(int handle);

    [JSImport("webgl.attachShader", "drawie.js")]
    public static partial void AttachShader(int handle, int program, int vertexShader);

    [JSImport("webgl.linkProgram", "drawie.js")]
    public static partial string? LinkProgram(int handle, int program);

    [JSImport("webgl.createBuffer", "drawie.js")]
    public static partial int CreateBuffer(int handle);

    [JSImport("webgl.bindBuffer", "drawie.js")]
    public static partial void BindBuffer(int handle, int array, int positionBuffer);

    [JSImport("webgl.bufferData", "drawie.js")]
    public static partial void BufferData(int handle, int arrayType, double[] vertices, int usage);

    [JSImport("webgl.clearColor", "drawie.js")]
    public static partial void ClearColor(int gl, double r, double g, double b, double a);

    [JSImport("webgl.clear", "drawie.js")]
    public static partial void Clear(int gl, int bits);

    [JSImport("webgl.vertexAttribPointer", "drawie.js")]
    public static partial void VertexAttribPointer(int gl, int index, int size, int type, bool normalized, int stride,
        int offset);

    [JSImport("webgl.enableVertexAttribArray", "drawie.js")]
    public static partial void EnableVertexAttribArray(int gl, int index);

    [JSImport("webgl.useProgram", "drawie.js")]
    public static partial void UseProgram(int gl, int program);

    [JSImport("webgl.drawArrays", "drawie.js")]
    public static partial void DrawArrays(int gl, int mode, int first, int count);

    [JSImport("webgl.getAttribLocation", "drawie.js")]
    public static partial int GetAttribLocation(int gl, int program, string name);

    [DllImport("libSkiaSharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InterceptGLObject();

    [JSImport("webgl.openSkiaContext", "drawie.js")]
    public static partial int OpenSkiaContext(string canvasObjectId);
    
    [JSImport("webgl.makeContextCurrent", "drawie.js")]
    public static partial void MakeContextCurrent(int contextHandle);

    [JSImport("webgl.createTexture", "drawie.js")]
    public static partial int CreateTexture(int handle);

    [JSImport("webgl.bindTexture", "drawie.js")]
    public static partial void BindTexture(int handle, int type, int textureId);

    [JSImport("webgl.texImage2D", "drawie.js")]
    public static partial void TexImage2D(int handle, int type, int level, int format, int width, int height,
        int border, int srcFormat, int srcType,
        int offset);

    [JSImport("webgl.texParameteri", "drawie.js")]
    public static partial void TexParameteri(int handle, int type, int pName, int wrapping);

    [JSImport("webgl.activeTexture", "drawie.js")]
    public static partial void ActiveTexture(int gl, int index);

    [JSImport("webgl.uniform1i", "drawie.js")]
    public static partial void Uniform1i(int gl, int location, int value);

    [JSImport("webgl.getUniformLocation", "drawie.js")]
    public static partial int GetUniformLocation(int gl, int program, string name);

    [JSImport("webgl.deleteTexture", "drawie.js")]
    public static partial void DeleteTexture(int gl, int textureId);
}
