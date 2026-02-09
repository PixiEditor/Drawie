using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Float3(string name) : ShaderExpressionVariable<Vec3D>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _xOverrideExpression;
    private Expression? _yOverrideExpression;
    private Expression? _zOverrideExpression;

    public override string ConstantValueString
    {
        get
        {
            string x = ConstantValue.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ConstantValue.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string z = ConstantValue.Z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"float3({x}, {y}, {z})";
        }
    }

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }

    public Float1 X
    {
        get
        {
            return new Float1($"{VariableName}.x")
            {
                ConstantValue = ConstantValue.X, OverrideExpression = _xOverrideExpression
            };
        }
    }

    public Float1 Y
    {
        get
        {
            return new Float1($"{VariableName}.y")
            {
                ConstantValue = ConstantValue.Y, OverrideExpression = _yOverrideExpression
            };
        }
    }

    public Float1 Z
    {
        get
        {
            return new Float1($"{VariableName}.z")
            {
                ConstantValue = ConstantValue.Z, OverrideExpression = _zOverrideExpression
            };
        }
    }

    public static implicit operator Float3(Vec3D value) => new Float3("") { ConstantValue = value };

    public static explicit operator Vec3D(Float3 value) => value.ConstantValue;

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => X,
            1 => Y,
            2 => Z,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void OverrideExpressionAt(int index, Expression? expression)
    {
        switch (index)
        {
            case 0:
                _xOverrideExpression = expression;
                break;
            case 1:
                _yOverrideExpression = expression;
                break;
            case 2:
                _zOverrideExpression = expression;
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public int GetValuesCount()
    {
        return 3;
    }

    public Expression? GetWholeNestedExpression()
    {
        return new Expression($"float3({X.ExpressionValue}, {Y.ExpressionValue}, {Z.ExpressionValue})");
    }

    public void OverrideConstantValueAt(int i, object constant)
    {
        switch (i)
        {
            case 0:
                if (constant is float x)
                {
                    ConstantValue = new Vec3D(x, ConstantValue.Y, ConstantValue.Z);
                }
                break;
            case 1:
                if (constant is float y)
                {
                    ConstantValue = new Vec3D(ConstantValue.X, y, ConstantValue.Z);
                }
                break;
            case 2:
                if (constant is float z)
                {
                    ConstantValue = new Vec3D(ConstantValue.X, ConstantValue.Y, z);
                }
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }
}
