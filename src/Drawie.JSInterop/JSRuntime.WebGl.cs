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
    public static partial void BufferData(int handle, int target, int size, int usage);
    
    [JSImport("webgl.bufferData", "drawie.js")]
    public static partial void BufferData(int handle, int arrayType, byte[] data, int usage);

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

    [JSImport("webgl.viewport", "drawie.js")]
    public static partial void Viewport(int gl, int x, int y, int width, int height);

    [JSImport("webgl.bindFramebuffer", "drawie.js")]
    public static partial void BindFramebuffer(int gl, int target, int framebuffer);

    [JSImport("webgl.createFramebuffer", "drawie.js")]
    public static partial int CreateFramebuffer(int gl);

    [JSImport("webgl.framebufferTexture2D", "drawie.js")]
    public static partial void FramebufferTexture2D(int gl, int target, int attachment, int textarget, int texture, int level);
    
    [JSImport("webgl.checkFramebufferStatus", "drawie.js")]
    public static partial int CheckFramebufferStatus(int gl, int target);

    [JSImport("webgl.getError", "drawie.js")]
    public static partial int GetError(int gl);

    [JSImport("webgl.deleteFramebuffer", "drawie.js")]
    public static partial void DeleteFramebuffer(int gl, int framebuffer);

    [JSImport("webgl.enable", "drawie.js")]
    public static partial void Enable(int gl, int cap);

    [JSImport("webgl.disable", "drawie.js")]
    public static partial void Disable(int gl, int cap);

    [JSImport("webgl.depthFunc", "drawie.js")]
    public static partial void DepthFunc(int gl, int func);
    
    [JSImport("webgl.clearDepth", "drawie.js")]
    public static partial void ClearDepth(int gl, double depth);

    [JSImport("webgl.depthMask", "drawie.js")]
    public static partial void DepthMask(int gl, bool value);

    [JSImport("webgl.getParameter", "drawie.js")]
    public static partial int GetParameter(int gl, int binding);

    [JSImport("webgl.bindVertexArray", "drawie.js")]
    public static partial void BindVertexArray(int gl, int vertexArrayHandle);

    [JSImport("webgl.bindSampler", "drawie.js")]
    public static partial void BindSampler(int gl, int slot, int samplerHandle);

    [JSImport("webgl.drawElements", "drawie.js")]
    public static partial void DrawElements(int gl, int mode, int count, int type, int offset);

    [JSImport("webgl.blitFramebuffer", "drawie.js")]
    public static partial void BlitFramebuffer(int gl, int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0,
        int dstX1, int dstY1, int mask, int filter);
    [JSImport("webgl.createSampler", "drawie.js")]
    public static partial int CreateSampler(int glHandle);

    [JSImport("webgl.createVertexArray", "drawie.js")]
    public static partial int CreateVertexArray(int gl);

    [JSImport("webgl.uniformBlockBinding", "drawie.js")]
    public static partial void UniformBlockBinding(int gl, int programHandle, int blockIndex, int bindingPoint);

    [JSImport("webgl.bindBufferBase", "drawie.js")]
    public static partial void BindBufferBase(int gl, int target, int bindingPoint, int buffer);

    [JSImport("webgl.bufferSubData", "drawie.js")]
    public static partial void BufferSubData(int gl, int target, int offset, byte[] data);

    [JSImport("webgl.createRenderbuffer", "drawie.js")]
    public static partial int CreateRenderbuffer(int api);

    [JSImport("webgl.bindRenderbuffer", "drawie.js")]
    public static partial void BindRenderbuffer(int gl, int target, int renderbuffer);

    [JSImport("webgl.deleteRenderbuffer", "drawie.js")]
    public static partial void DeleteRenderbuffer(int gl, int renderbuffer);

    [JSImport("webgl.renderbufferStorage", "drawie.js")]
    public static partial void RenderbufferStorage(int gl, int target, int internalFormat, int width, int height);
    
    [JSImport("webgl.framebufferRenderbuffer", "drawie.js")]
    public static partial void FramebufferRenderbuffer(int gl, int target, int attachment, int renderbufferTarget, int renderbuffer);

    [JSImport("webgl.getContext", "drawie.js")]
    public static partial int GetContext(string canvasId, string ctx);
}
