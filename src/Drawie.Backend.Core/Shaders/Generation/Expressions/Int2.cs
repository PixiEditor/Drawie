using Drawie.Numerics;

namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class Int2(string name) : ShaderExpressionVariable<VecI>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    private Expression? _overrideExpressionX;
    private Expression? _overrideExpressionY;
    
    public override string ConstantValueString => $"int2({ConstantValue.X}, {ConstantValue.Y})";

    public Int1 X
    {
        get => new Int1($"{VariableName}.x") { ConstantValue = ConstantValue.X, OverrideExpression = _overrideExpressionX };
    }

    public Int1 Y
    {
        get => new Int1($"{VariableName}.y") { ConstantValue = ConstantValue.Y, OverrideExpression = _overrideExpressionY };
    }

    public static implicit operator Int2(VecI value) => new Int2("") { ConstantValue = value };
    public static explicit operator VecI(Int2 value) => value.ConstantValue;
    
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
                _overrideExpressionX = expression;
                break;
            case 1:
                _overrideExpressionY = expression;
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
        return new Expression($"int2({X.ExpressionValue}, {Y.ExpressionValue})");
    }

    public void OverrideConstantValueAt(int i, object constant)
    {
        switch (i)
        {
            case 0:
                if (constant is int x)
                {
                    ConstantValue = new VecI(x, ConstantValue.Y);
                }
                break;
            case 1:
                if (constant is int y)
                {
                    ConstantValue = new VecI(ConstantValue.X, y);
                }
                break;
            default:
                throw new IndexOutOfRangeException();
        }
    }
}
