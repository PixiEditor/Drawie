using System.Text;
using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace Drawie.Backend.Core.Shaders.Generation;

public partial class BuiltInFunctions
{
    private readonly List<IBuiltInFunction> usedFunctions = new(38);

    public Expression GetRgbToHsv(Expression rgba) => Call(RgbToHsv, rgba);

    public Expression GetRgbToHsl(Expression rgba) => Call(RgbToHsl, rgba);

    public Expression GetHsvToRgb(Expression hsva) => Call(HsvToRgb, hsva);

    public Expression GetHsvToRgb(Expression h, Expression s, Expression v, Expression a) =>
        GetHsvToRgb(Half4Float1Accessor.GetOrConstructorExpressionHalf4(h, s, v, a));

    public Expression GetHslToRgb(Expression hsla) => Call(HslToRgb, hsla);

    public Expression GetHslToRgb(Expression h, Expression s, Expression l, Expression a) =>
        GetHslToRgb(Half4Float1Accessor.GetOrConstructorExpressionHalf4(h, s, l, a));

    public Expression GetRemap(Expression value, Expression oldMin, Expression oldMax, Expression newMin, Expression newMax)
    {
        return Call(Remap, new Expression($"{value.ExpressionValue}, {oldMin.ExpressionValue}, {oldMax.ExpressionValue}, {newMin.ExpressionValue}, {newMax.ExpressionValue}"));
    }

    public string BuildFunctions()
    {
        var builder = new StringBuilder();

        foreach (var function in usedFunctions)
        {
            builder.AppendLine(function.FullSource);
        }

        return builder.ToString();
    }

    private Expression Call(IBuiltInFunction function, Expression expression)
    {
        Require(function);

        return new Expression(function.Call(expression.ExpressionValue));
    }

    private void Require(IBuiltInFunction function)
    {
        if (usedFunctions.Contains(function))
        {
            return;
        }

        foreach (var dependency in function.Dependencies)
        {
            Require(dependency);
        }

        usedFunctions.Add(function);
    }

    // Taken from here https://www.shadertoy.com/view/4dKcWK
    private static readonly BuiltInFunction<Half3> HueToRgb = new(
        "float hue",
        nameof(HueToRgb),
        """
        hue = fract(hue) * 6.0;
        half3 rgb;
        if (hue < 1.0)       rgb = half3(1.0, hue, 0.0);
        else if (hue < 2.0)  rgb = half3(2.0 - hue, 1.0, 0.0);
        else if (hue < 3.0)  rgb = half3(0.0, 1.0, hue - 2.0);
        else if (hue < 4.0)  rgb = half3(0.0, 4.0 - hue, 1.0);
        else if (hue < 5.0)  rgb = half3(hue - 4.0, 0.0, 1.0);
        else                 rgb = half3(1.0, 0.0, 6.0 - hue);
        return rgb; // no clamp here
        """);


    private static readonly BuiltInFunction<Half3> RgbToHcv = new(
        "half3 rgb",
        nameof(RgbToHcv),
        """
        half4 p = (rgb.g < rgb.b) ? half4(rgb.bg, -1.0, 2.0/3.0) : half4(rgb.gb, 0.0, -1.0/3.0);
        half4 q = (rgb.r < p.x) ? half4(p.x, p.y, p.w, rgb.r) : half4(rgb.r, p.y, p.z, p.x);
        float c = q.x - min(q.w, q.y);
        if (c <= 0.0) return half3(0.0, 0.0, q.x); // grayscale safe
        float h = (q.w - q.y) / (6.0 * c) + q.z;
        return half3(fract(h), c, q.x);
        """);


    private static readonly BuiltInFunction<Half4> RgbToHsv = new(
        "half4 rgba",
        nameof(RgbToHsv),
        $"""
         half3 hcv = {RgbToHcv.Call("rgba.rgb")};
         float s = hcv.y / (hcv.z);
         return half4(hcv.x, s, hcv.z, rgba.w);
         """,
        RgbToHcv);

    private static readonly BuiltInFunction<Half4> HsvToRgb = new(
        "half4 hsva",
        nameof(HsvToRgb),
        $"""
         half3 rgb = {HueToRgb.Call("hsva.r")};
         return half4(((rgb - 1.) * hsva.y + 1.) * hsva.z, hsva.w);
         """,
        HueToRgb);

    private static readonly BuiltInFunction<Half4> RgbToHsl = new(
        "half4 rgba",
        nameof(RgbToHsl),
        """
        half3 hcv = RgbToHcv(rgba.rgb);
        half L = hcv.z - hcv.y * 0.5;
        half denom = 1.0 - abs(2.0 * L - 1.0);
        half S = denom > 0.0 ? hcv.y / denom : 0.0;
        return half4(hcv.x, S, L, rgba.w);
        """,
        RgbToHcv);


    private static readonly BuiltInFunction<Half4> HslToRgb = new(
        "half4 hsla",
        nameof(HslToRgb),
        """
        half3 rgb = HueToRgb(hsla.x);
        float C = (1.0 - abs(2.0 * hsla.z - 1.0)) * hsla.y;
        float m = hsla.z - 0.5 * C;
        return half4(rgb * C + m, hsla.w);
        """,
        HueToRgb);


    private static readonly BuiltInFunction<Float1> Remap = new(
        "float value, float oldMin, float oldMax, float newMin, float newMax",
        nameof(Remap),
        """
        return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
        """);

    private class BuiltInFunction<TReturn>(
        string argumentList,
        string name,
        string body,
        params IBuiltInFunction[] dependencies) : IBuiltInFunction where TReturn : ShaderExpressionVariable
    {
        public string ArgumentList { get; } = argumentList;

        public string Name { get; } = name;

        public string Body { get; } = body;

        public IBuiltInFunction[] Dependencies { get; } = dependencies;

        public string FullSource =>
            $$"""
              {{GetReturnType()}} {{Name}}({{ArgumentList}}) {
              {{Body}}
              }
              """;

        private static string GetReturnType()
        {
            if (typeof(TReturn) == typeof(Float1))
            {
                return "float";
            }

            return typeof(TReturn).Name.ToLower();
        }

        public string Call(string arguments) => $"{Name}({arguments})";
    }

    private interface IBuiltInFunction
    {
        IBuiltInFunction[] Dependencies { get; }

        string Name { get; }

        string FullSource { get; }

        string Call(string arguments);
    }
}
