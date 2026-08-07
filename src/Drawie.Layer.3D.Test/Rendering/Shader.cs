using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkNet.Rendering;

public sealed class Shader : NativeObject
{
    public Shader(GL glContext, string rawVertex, string rawFragment) : base(glContext, glContext.CreateProgram())
    {
        uint vertex = InitShader(ShaderType.VertexShader, rawVertex);
        uint fragment = InitShader(ShaderType.FragmentShader, rawFragment);

        GlContext.AttachShader(Handle, vertex);
        GlContext.AttachShader(Handle, fragment);
        GlContext.LinkProgram(Handle);
        
        GlContext.GetProgram(Handle, GLEnum.LinkStatus, out var status);

        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {GlContext.GetProgramInfoLog(Handle)}");
        }
        
        GlContext.DetachShader(Handle, vertex);
        GlContext.DetachShader(Handle, fragment);
        GlContext.DeleteShader(vertex);
        GlContext.DeleteShader(fragment);
    }

    public void Use()
    {
        GlContext.UseProgram(Handle);
    }

    // Uniforms are properties that applies to the entire geometry
    public void SetUniform(string name, int value)
    {
        int location = GetUniformLocation(name);
        GlContext.Uniform1(location, value);
    }
    
    public void SetUniform(string name, float value)
    {
        int location = GetUniformLocation(name);
        GlContext.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, Matrix4x4 transformViewMatrix)
    {
        int location = GetUniformLocation(name);
        GlContext.UniformMatrix4(location, 1, false, (float*)&transformViewMatrix);
    }
    
    public void SetUniform(string name, Vector3 value)
    {
        int location = GetUniformLocation(name);
        GlContext.Uniform3(location, value.X, value.Y, value.Z);
    }

    private int GetUniformLocation(string name)
    {
        int location = GlContext.GetUniformLocation(Handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on the shader.");
        }

        return location;
    }
    
    public bool HasUniform(string uniformName)
    {
        return GlContext.GetUniformLocation(Handle, uniformName) != -1;
    }

    private uint InitShader(ShaderType type, string raw)
    {
        uint handle = GlContext.CreateShader(type);
        GlContext.ShaderSource(handle, raw);
        GlContext.CompileShader(handle);
        string infoLog = GlContext.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Compiling shader of type {type} failed with error {infoLog}");
        }

        return handle;
    }

    public override void Dispose()
    {
        GlContext.DeleteProgram(Handle);
    }
}