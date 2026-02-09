using Drawie.Backend.Core.ColorsImpl;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Half4(string name) : ShaderExpressionVariable<Color>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _rOverrideExpression;
    private Expression? _gOverrideExpression;
    private Expression? _bOverrideExpression;
    private Expression? _aOverrideExpression;

    public override string ConstantValueString =>
        $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";

    public Float1 R =>
        new Half4Float1Accessor(this, 'r')
        {
            ConstantValue = ConstantValue.R, OverrideExpression = _rOverrideExpression
        };

    public Float1 G =>
        new Half4Float1Accessor(this, 'g')
        {
            ConstantValue = ConstantValue.G, OverrideExpression = _gOverrideExpression
        };

    public Float1 B =>
        new Half4Float1Accessor(this, 'b')
        {
            ConstantValue = ConstantValue.B, OverrideExpression = _bOverrideExpression
        };

    public Float1 A =>
        new Half4Float1Accessor(this, 'a')
        {
            ConstantValue = ConstantValue.A, OverrideExpression = _aOverrideExpression
        };

    public static implicit operator Half4(Color value) => new("") { ConstantValue = value };
    public static explicit operator Color(Half4 value) => value.ConstantValue;

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => R,
            1 => G,
            2 => B,
            3 => A,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public void OverrideExpressionAt(int index, Expression? expression)
    {
        switch (index)
        {
            case 0:
                _rOverrideExpression = expression;
                break;
            case 1:
                _gOverrideExpression = expression;
                break;
            case 2:
                _bOverrideExpression = expression;
                break;
            case 3:
                _aOverrideExpression = expression;
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public int GetValuesCount()
    {
        return 4;
    }

    public Expression? GetWholeNestedExpression() => Constructor(R, G, B, A);
    public void OverrideConstantValueAt(int i, object constant)
    {
        if (constant is byte byteValue)
        {
            switch (i)
            {
                case 0:
                    ConstantValue = new Color(byteValue, ConstantValue.G, ConstantValue.B, ConstantValue.A);
                    break;
                case 1:
                    ConstantValue = new Color(ConstantValue.R, byteValue, ConstantValue.B, ConstantValue.A);
                    break;
                case 2:
                    ConstantValue = new Color(ConstantValue.R, ConstantValue.G, byteValue, ConstantValue.A);
                    break;
                case 3:
                    ConstantValue = new Color(ConstantValue.R, ConstantValue.G, ConstantValue.B, byteValue);
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    public static string ConstructorText(Expression r, Expression g, Expression b, Expression a) =>
        $"half4({r.ExpressionValue}, {g.ExpressionValue}, {b.ExpressionValue}, {a.ExpressionValue})";

    public static Expression Constructor(Expression r, Expression g, Expression b, Expression a) =>
        new Expression(ConstructorText(r, g, b, a));
}
