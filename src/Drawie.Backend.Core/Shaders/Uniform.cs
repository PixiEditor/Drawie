using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders;

public struct Uniform
{
    public string Name { get; }
    public float FloatValue { get; }
    public float[] FloatArrayValue { get; }
    public Shader ShaderValue { get; }
    public Color ColorValue { get; }
    public VecD Vector2Value { get; }
    public Vec3D Vector3Value { get; }
    public Vec4D Vector4Value { get; }
    public string UniformName { get; }

    public string LayoutOf { get; } = string.Empty;

    public UniformValueType DataType { get; }

    public Uniform(string name, float value)
    {
        Name = name;
        FloatValue = value;
        DataType = UniformValueType.Float;
        UniformName = "float";
    }

    public Uniform(string name, VecD vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y };
        DataType = UniformValueType.Vector2;
        Vector2Value = vector;
        UniformName = "float2";
    }

    public Uniform(string name, Vec3D vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y, (float)vector.Z };
        DataType = UniformValueType.Vector3;
        Vector3Value = vector;
        UniformName = "float3";
    }

    public Uniform(string name, Vec4D vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W };
        Vector4Value = vector;
        DataType = UniformValueType.Vector4;
        UniformName = "float4";
    }

    public Uniform(string name, Shader value)
    {
        Name = name;
        ShaderValue = value;
        DataType = UniformValueType.Shader;
        UniformName = "shader";
    }

    public Uniform(string name, Color color)
    {
        Name = name;
        FloatArrayValue = new float[] { color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f };
        ColorValue = color;
        DataType = UniformValueType.Color;
        LayoutOf = "color";
        UniformName = "half4";
    }

    public void Dispose()
    {
        ShaderValue?.Dispose();
    }
}

public enum UniformValueType
{
    Float,
    FloatArray,
    Shader,
    Color,
    Vector2,
    Vector3,
    Vector4
}
