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
        IntValue = (int)value;
        DataType = UniformValueType.Float;
        UniformName = "float";
    }

    public Uniform(string name, VecD vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y };
        IntArrayValue = new int[] { (int)vector.X, (int)vector.Y };
        DataType = UniformValueType.Vector2;
        Vector2Value = vector;
        UniformName = "float2";
    }

    public Uniform(string name, Vec3D vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y, (float)vector.Z };
        IntArrayValue = new int[] { (int)vector.X, (int)vector.Y, (int)vector.Z };
        DataType = UniformValueType.Vector3;
        Vector3Value = vector;
        UniformName = "float3";
    }

    public Uniform(string name, Vec4D vector)
    {
        Name = name;
        FloatArrayValue = new float[] { (float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W };
        IntArrayValue = new int[] { (int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W };
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
        FloatValue = value;
        UniformName = "int";
    }

    public Uniform(string name, VecI vector)
    {
        Name = name;
        IntArrayValue = new int[] { vector.X, vector.Y };
        DataType = UniformValueType.Vector2Int;
        Vector2IntValue = vector;
        Vector2Value = new VecD(vector.X, vector.Y);
        UniformName = "int2";
    }

    public Uniform(string name, int[] vector)
    {
        Name = name;
        IntArrayValue = vector;
        FloatArrayValue = vector.Select(i => (float)i).ToArray();
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
        FloatArrayValue = matrix.ValuesColumnMajor;
        DataType = UniformValueType.Matrix3X3;
        UniformName = "float3x3";
    }

    public int GetContentHash()
    {
        return DataType switch
        {
            UniformValueType.Float => FloatValue.GetHashCode(),
            UniformValueType.Int => IntValue.GetHashCode(),
            UniformValueType.FloatArray => HashCode.Combine(FloatArrayValue.Length,
                FloatArrayValue.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()))),
            UniformValueType.IntArray => HashCode.Combine(IntArrayValue.Length,
                IntArrayValue.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()))),
            UniformValueType.Shader => (int)(ShaderValue?.ObjectPointer ?? 0),
            UniformValueType.Color => ColorValue.GetHashCode(),
            UniformValueType.Vector2 => Vector2Value.GetHashCode(),
            UniformValueType.Vector3 => Vector3Value.GetHashCode(),
            UniformValueType.Vector4 => Vector4Value.GetHashCode(),
            UniformValueType.Vector2Int => Vector2IntValue.GetHashCode(),
            UniformValueType.Vector3Int => 0, // TODO: implement
            UniformValueType.Vector4Int => 0, // TODO: implement
            UniformValueType.Matrix3X3 => HashCode.Combine(FloatArrayValue.Length,
                FloatArrayValue.Aggregate(0, (a, b) => HashCode.Combine(a, b.GetHashCode()))),
            _ => 0
        };
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
