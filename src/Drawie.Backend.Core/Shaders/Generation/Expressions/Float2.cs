using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Float2(string name) : ShaderExpressionVariable<VecD>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _xOverrideExpression;
    private Expression? _yOverrideExpression;

    public override string ConstantValueString
    {
        get
        {
            string x = ConstantValue.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ConstantValue.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"float2({x}, {y})";
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

    public static implicit operator Float2(VecD value) => new Float2("") { ConstantValue = value };

    public static explicit operator VecD(Float2 value) => value.ConstantValue;

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => X,
            1 => Y,
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
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public int GetValuesCount()
    {
        return 2;
    }

    public Expression? GetWholeNestedExpression()
    {
        return new Expression($"float2({X.ExpressionValue}, {Y.ExpressionValue})");
    }

    public void OverrideConstantValueAt(int i, object constant)
    {
        if (constant is double doubleValue)
        {
            switch (i)
            {
                case 0:
                    ConstantValue = new VecD(doubleValue, ConstantValue.Y);
                    break;
                case 1:
                    ConstantValue = new VecD(ConstantValue.X, doubleValue);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
