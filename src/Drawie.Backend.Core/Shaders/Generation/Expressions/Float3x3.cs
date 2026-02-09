using Drawie.Backend.Core.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Float3x3(string name) : ShaderExpressionVariable<Matrix3X3>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? overrideExpressionM11;
    private Expression? overrideExpressionM12;
    private Expression? overrideExpressionM13;
    private Expression? overrideExpressionM21;
    private Expression? overrideExpressionM22;
    private Expression? overrideExpressionM23;
    private Expression? overrideExpressionM31;
    private Expression? overrideExpressionM32;
    private Expression? overrideExpressionM33;

    public Float1 M11 =>
        new Float3x3Float1Accesor(this, 0, 0)
        {
            ConstantValue = ConstantValue.Values[0], OverrideExpression = overrideExpressionM11
        };

    public Float1 M12 =>
        new Float3x3Float1Accesor(this, 0, 1)
        {
            ConstantValue = ConstantValue.Values[1], OverrideExpression = overrideExpressionM12
        };

    public Float1 M13 =>
        new Float3x3Float1Accesor(this, 0, 2)
        {
            ConstantValue = ConstantValue.Values[2], OverrideExpression = overrideExpressionM13
        };

    public Float1 M21 =>
        new Float3x3Float1Accesor(this, 1, 0)
        {
            ConstantValue = ConstantValue.Values[3], OverrideExpression = overrideExpressionM21
        };

    public Float1 M22 =>
        new Float3x3Float1Accesor(this, 1, 1)
        {
            ConstantValue = ConstantValue.Values[4], OverrideExpression = overrideExpressionM22
        };

    public Float1 M23 =>
        new Float3x3Float1Accesor(this, 1, 2)
        {
            ConstantValue = ConstantValue.Values[5], OverrideExpression = overrideExpressionM23
        };

    public Float1 M31 =>
        new Float3x3Float1Accesor(this, 2, 0)
        {
            ConstantValue = ConstantValue.Values[6], OverrideExpression = overrideExpressionM31
        };

    public Float1 M32 =>
        new Float3x3Float1Accesor(this, 2, 1)
        {
            ConstantValue = ConstantValue.Values[7], OverrideExpression = overrideExpressionM32
        };

    public Float1 M33 =>
        new Float3x3Float1Accesor(this, 2, 2)
        {
            ConstantValue = ConstantValue.Values[8], OverrideExpression = overrideExpressionM33
        };

    public override string ConstantValueString =>
        $"float3x3({ConstantValue.Values[0]}, {ConstantValue.Values[1]}, {ConstantValue.Values[2]}, " +
        $"{ConstantValue.Values[3]}, {ConstantValue.Values[4]}, {ConstantValue.Values[5]}, " +
        $"{ConstantValue.Values[6]}, {ConstantValue.Values[7]}, {ConstantValue.Values[8]})";

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set => _overrideExpression = value;
    }

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => M11,
            1 => M12,
            2 => M13,
            3 => M21,
            4 => M22,
            5 => M23,
            6 => M31,
            7 => M32,
            8 => M33,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void OverrideExpressionAt(int index, Expression? expression)
    {
        switch (index)
        {
            case 0:
                overrideExpressionM11 = expression;
                break;
            case 1:
                overrideExpressionM12 = expression;
                break;
            case 2:
                overrideExpressionM13 = expression;
                break;
            case 3:
                overrideExpressionM21 = expression;
                break;
            case 4:
                overrideExpressionM22 = expression;
                break;
            case 5:
                overrideExpressionM23 = expression;
                break;
            case 6:
                overrideExpressionM31 = expression;
                break;
            case 7:
                overrideExpressionM32 = expression;
                break;
            case 8:
                overrideExpressionM33 = expression;
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public int GetValuesCount()
    {
        return 9;
    }

    public Expression? GetWholeNestedExpression()
    {
        return new Expression($"float3x3({M11}, {M12}, {M13}, {M21}, {M22}, {M23}, {M31}, {M32}, {M33})");
    }

    public void OverrideConstantValueAt(int i, object constant)
    {
        if (constant is double doubleValue)
        {
            var values = ConstantValue.Values;
            values[i] = (float)doubleValue;
            ConstantValue = new Matrix3X3(values);
        }
        else if (constant is float floatValue)
        {
            var values = ConstantValue.Values;
            values[i] = floatValue;
            ConstantValue = new Matrix3X3(values);
        }
        else if (constant is int intValue)
        {
            var values = ConstantValue.Values;
            values[i] = intValue;
            ConstantValue = new Matrix3X3(values);
        }
    }
}
