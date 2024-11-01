using System.Runtime.InteropServices.JavaScript;

namespace Drawie.JSInterop;

public partial class JSRuntime
{
    [JSImport("webgl.createShader", "main.js")]
    public static partial int CreateShader(int contextHandle, int shaderType);

    [JSImport("webgl.shaderSource", "main.js")]
    public static partial void ShaderSource(int handle, int shaderHandle, string shader);

    [JSImport("webgl.compileShader", "main.js")]
    public static partial string? CompileShader(int handle, int shaderHandle);

    [JSImport("webgl.createProgram", "main.js")]
    public static partial int CreateProgram(int handle);

    [JSImport("webgl.attachShader", "main.js")]
    public static partial void AttachShader(int handle, int program, int vertexShader);

    [JSImport("webgl.linkProgram", "main.js")]
    public static partial string? LinkProgram(int handle, int program);

    [JSImport("webgl.createBuffer", "main.js")]
    public static partial int CreateBuffer(int handle);

    [JSImport("webgl.bindBuffer", "main.js")]
    public static partial void BindBuffer(int handle, int array, int positionBuffer);

    [JSImport("webgl.bufferData", "main.js")]
    public static partial void BufferData(int handle, int arrayType, double[] vertices, int usage);

    [JSImport("webgl.clearColor", "main.js")]
    public static partial void ClearColor(int gl, double r, double g, double b, double a);

    [JSImport("webgl.clear", "main.js")]
    public static partial void Clear(int gl, int bits);

    [JSImport("webgl.vertexAttribPointer", "main.js")]
    public static partial void VertexAttribPointer(int gl, int index, int size, int type, bool normalized, int stride, int offset);

    [JSImport("webgl.enableVertexAttribArray", "main.js")]
    public static partial void EnableVertexAttribArray(int gl, int index);

    [JSImport("webgl.useProgram", "main.js")]
    public static partial void UseProgram(int gl, int program);
    
    [JSImport("webgl.drawArrays", "main.js")]
    public static partial void DrawArrays(int gl, int mode, int first, int count);

    [JSImport("webgl.getAttribLocation", "main.js")]
    public static partial int GetAttribLocation(int gl, int program, string name);

    [JSImport("webgl.getProcAddress", "main.js")]
    public static partial IntPtr GetProcAddress(string name);
}
