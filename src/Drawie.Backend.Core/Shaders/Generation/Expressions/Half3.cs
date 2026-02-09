using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Half3(string name) : ShaderExpressionVariable<Vec3D>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _rOverrideExpression;
    private Expression? _gOverrideExpression;
    private Expression? _bOverrideExpression;
    
    public override string ConstantValueString => $"half3({ConstantValue.X}, {ConstantValue.Y}, {ConstantValue.Z})";

    public Float1 R =>
        new Half3Float1Accessor(this, 'r')
        {
            ConstantValue = ConstantValue.X, OverrideExpression = _rOverrideExpression
        };

    public Float1 G =>
        new Half3Float1Accessor(this, 'g')
        {
            ConstantValue = ConstantValue.X, OverrideExpression = _gOverrideExpression
        };

    public Float1 B =>
        new Half3Float1Accessor(this, 'b')
        {
            ConstantValue = ConstantValue.Z, OverrideExpression = _bOverrideExpression
        };

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
        return Constructor(R, G, B);
    }

    public void OverrideConstantValueAt(int i, object constant)
    {
        switch (i)
        {
            case 0:
                if (constant is float r)
                {
                    ConstantValue = new Vec3D(r, ConstantValue.Y, ConstantValue.Z);
                }
                break;
            case 1:
                if (constant is float g)
                {
                    ConstantValue = new Vec3D(ConstantValue.X, g, ConstantValue.Z);
                }
                break;
            case 2:
                if (constant is float b)
                {
                    ConstantValue = new Vec3D(ConstantValue.X, ConstantValue.Y, b);
                }
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }

    public static string ConstructorText(Expression r, Expression g, Expression b) =>
        $"half3({r.ExpressionValue}, {g.ExpressionValue}, {b.ExpressionValue})";

    public static Expression Constructor(Expression r, Expression g, Expression b) => new(ConstructorText(r, g, b));
}
