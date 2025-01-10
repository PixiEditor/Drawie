namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public static class ShaderMath
{
    public static Expression Add(Expression a, Expression b)
    {
        return new Expression($"{a.ExpressionValue} + {b.ExpressionValue}");
    }

    public static Expression Subtract(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.ExpressionValue} - {b.ExpressionValue}");
    }

    public static Expression Multiply(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.ExpressionValue} * {b.ExpressionValue}");
    }

    public static Expression Divide(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.ExpressionValue} / {b.ExpressionValue}");
    }

    public static Expression Clamp(Expression value, Expression min, Expression max)
    {
        return new Expression($"clamp({value.ExpressionValue}, {min.ExpressionValue}, {max.ExpressionValue})");
    }

    public static Expression Sin(Expression x)
    {
        return new Expression($"sin({x.ExpressionValue})");
    }

    public static Expression Cos(Expression x)
    {
        return new Expression($"cos({x.ExpressionValue})");
    }

    public static Expression Tan(Expression x)
    {
        return new Expression($"tan({x.ExpressionValue})");
    }

    public static Expression GreaterThan(Expression a, Expression b)
    {
        return new Expression($"float({a.ExpressionValue} > {b.ExpressionValue})");
    }

    public static Expression GreaterThanOrEqual(Expression a, Expression b)
    {
        return new Expression($"float({a.ExpressionValue} >= {b.ExpressionValue})");
    }

    public static Expression LessThan(Expression a, Expression b)
    {
        return new Expression($"float({a.ExpressionValue} < {b.ExpressionValue})");
    }

    public static Expression LessThanOrEqual(Expression a, Expression b)
    {
        return new Expression($"float({a.ExpressionValue} <= {b.ExpressionValue})");
    }

    public static Expression Lerp(Expression a, Expression b, Expression t)
    {
        return new Expression($"mix({a.ExpressionValue}, {b.ExpressionValue}, {t.ExpressionValue})");
    }

    public static Expression Compare(ShaderExpressionVariable a, ShaderExpressionVariable b, ShaderExpressionVariable t)
    {
        return new Expression($"float(abs({a.VarOrConst()} - {b.VarOrConst()}) < {t.VarOrConst()})");
    }

    public static Expression Power(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"pow({a.VarOrConst()}, {b.VarOrConst()})");
    }

    public static Expression Log(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        var baseConstant = Convert.ToDouble(b.GetConstant());

        return Math.Abs(baseConstant - 2) < 0.00000001
            ? new Expression($"log2({a.VarOrConst()}, {b.VarOrConst()})")
            : new Expression($"log({a.VarOrConst()}) / log({b.VarOrConst()})");
    }

    public static Expression LogE(ShaderExpressionVariable a)
    {
        return new Expression($"log({a.VarOrConst()})");
    }

    public static Expression Root(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        var baseConstant = Convert.ToDouble(b.GetConstant());

        return Math.Abs(baseConstant - 2) < 0.00000001
            ? new Expression($"sqrt({a.VarOrConst()})")
            : new Expression($"pow({a.VarOrConst()}, 1.0 / {b.VarOrConst()})");
    }

    public static Expression InverseRoot(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"1 / {Root(a, b).ExpressionValue}");
    }

    public static Expression Fraction(ShaderExpressionVariable a)
    {
        return new Expression($"fract({a.VarOrConst()})");
    }

    public static Expression Abs(ShaderExpressionVariable a)
    {
        return new Expression($"abs({a.VarOrConst()})");
    }

    public static Expression Negate(ShaderExpressionVariable a)
    {
        return new Expression($"-({a.VarOrConst()})");
    }

    public static Expression Floor(ShaderExpressionVariable a)
    {
        return new Expression($"floor({a.VarOrConst()})");
    }

    public static Expression Ceil(ShaderExpressionVariable a)
    {
        return new Expression($"ceil({a.VarOrConst()})");
    }

    // Seems like there is no 'round' function in SKSL, or it is a bug, that's why floor is used instead
    public static Expression Round(ShaderExpressionVariable a)
    {
        return new Expression($"floor({a.VarOrConst()} + 0.5)");
    }

    public static Expression Modulo(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"mod({a.VarOrConst()}, {b.VarOrConst()})");
    }

    public static Expression Min(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"min({a.VarOrConst()}, {b.VarOrConst()})");
    }

    public static Expression Max(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"max({a.VarOrConst()}, {b.VarOrConst()})");
    }

    public static Expression Step(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"step({a.VarOrConst()}, {b.VarOrConst()})");
    }
    
    public static Expression SmoothStep(ShaderExpressionVariable a, ShaderExpressionVariable b, ShaderExpressionVariable c)
    {
        return new Expression($"smoothstep({a.VarOrConst()}, {b.VarOrConst()}, {c.VarOrConst()})");
    }
}
