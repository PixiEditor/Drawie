using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders;

public struct Uniform
{
    public string Name { get; }
    public float FloatValue { get; }

    public int IntValue { get; }
    public float[] FloatArrayValue { get; }

    public int[] IntArrayValue { get; }
    public Shader ShaderValue { get; }
    public Color ColorValue { get; }
    public VecD Vector2Value { get; }
    public VecI Vector2IntValue { get; }
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

    public Uniform(string name, int value)
    {
        Name = name;
        IntValue = value;
        DataType = UniformValueType.Int;
        UniformName = "int";
    }

    public Uniform(string name, VecI vector)
    {
        Name = name;
        IntArrayValue = new int[] { vector.X, vector.Y };
        DataType = UniformValueType.Vector2Int;
        Vector2IntValue = vector;
        UniformName = "int2";
    }

    public Uniform(string name, int[] vector)
    {
        Name = name;
        IntArrayValue = vector;
        DataType = UniformValueType.IntArray;
        UniformName = vector.Length switch
        {
            3 => "int3",
            4 => "int4",
            _ => throw new ArgumentException("Invalid length")
        };
    }

    public Uniform(string name, Matrix3X3 matrix)
    {
        Name = name;
        FloatArrayValue = matrix.Values;
        DataType = UniformValueType.Matrix3X3;
        UniformName = "float3x3";
    }

    public void Dispose()
    {
        ShaderValue?.Dispose();
    }
}

public enum UniformValueType
{
    Float,
    Int,
    FloatArray,
    IntArray,
    Shader,
    Color,
    Vector2,
    Vector3,
    Vector4,
    Vector2Int,
    Vector3Int,
    Vector4Int,
    Matrix3X3,
}
