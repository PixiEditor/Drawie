using Drawie.Backend.Core.Shaders.Generation.Expressions;

namespace Drawie.Backend.Core.Shaders.Generation;

public partial class BuiltInFunctions
{
    public Expression GetInSine(Float1 x) => Call(InSine, x);
    public Expression GetOutSine(Float1 x) => Call(OutSine, x);
    public Expression GetInOutSine(Float1 x) => Call(InOutSine, x);
    public Expression GetInQuad(Float1 x) => Call(InQuad, x);
    public Expression GetOutQuad(Float1 x) => Call(OutQuad, x);
    public Expression GetInOutQuad(Float1 x) => Call(InOutQuad, x);
    public Expression GetInCubic(Float1 x) => Call(InCubic, x);
    public Expression GetOutCubic(Float1 x) => Call(OutCubic, x);
    public Expression GetInOutCubic(Float1 x) => Call(InOutCubic, x);
    public Expression GetInQuart(Float1 x) => Call(InQuart, x);
    public Expression GetOutQuart(Float1 x) => Call(OutQuart, x);
    public Expression GetInOutQuart(Float1 x) => Call(InOutQuart, x);
    public Expression GetInQuint(Float1 x) => Call(InQuint, x);
    public Expression GetOutQuint(Float1 x) => Call(OutQuint, x);
    public Expression GetInOutQuint(Float1 x) => Call(InOutQuint, x);
    public Expression GetInExpo(Float1 x) => Call(InExpo, x);
    public Expression GetOutExpo(Float1 x) => Call(OutExpo, x);
    public Expression GetInOutExpo(Float1 x) => Call(InOutExpo, x);
    public Expression GetInCirc(Float1 x) => Call(InCirc, x);
    public Expression GetOutCirc(Float1 x) => Call(OutCirc, x);
    public Expression GetInOutCirc(Float1 x) => Call(InOutCirc, x);
    public Expression GetInBack(Float1 x) => Call(InBack, x);
    public Expression GetOutBack(Float1 x) => Call(OutBack, x);
    public Expression GetInOutBack(Float1 x) => Call(InOutBack, x);
    public Expression GetInElastic(Float1 x) => Call(InElastic, x);
    public Expression GetOutElastic(Float1 x) => Call(OutElastic, x);
    public Expression GetInOutElastic(Float1 x) => Call(InOutElastic, x);
    public Expression GetInBounce(Float1 x) => Call(InBounce, x);
    public Expression GetOutBounce(Float1 x) => Call(OutBounce, x);
    public Expression GetInOutBounce(Float1 x) => Call(InOutBounce, x);

    private static string Pi => "3.1415926535897932384626433832795";
    private static string HalfPi => "1.5707963267948966192313216916398";

    private static readonly BuiltInFunction<Float1> InSine = new(
        "float x",
        nameof(InSine),
        $"return 1. - cos(x * {HalfPi});");

    private static readonly BuiltInFunction<Float1> OutSine = new(
        "float x",
        nameof(OutSine),
        $"return sin(x * {HalfPi});");

    private static readonly BuiltInFunction<Float1> InOutSine = new(
        "float x",
        nameof(InOutSine),
        $"return -0.5 * (cos({Pi} * x) - 1);");

    private static readonly BuiltInFunction<Float1> InQuad = new(
        "float x",
        nameof(InQuad),
        "return x * x;");

    private static readonly BuiltInFunction<Float1> OutQuad = new(
        "float x",
        nameof(OutQuad),
        "return x * (2 - x);");

    private static readonly BuiltInFunction<Float1> InOutQuad = new(
        "float x",
        nameof(InOutQuad),
        "return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) * 0.5;");

    private static readonly BuiltInFunction<Float1> InCubic = new(
        "float x",
        nameof(InCubic),
        "return x * x * x;");

    private static readonly BuiltInFunction<Float1> OutCubic = new(
        "float x",
        nameof(OutCubic),
        "return pow(x - 1, 3) + 1;");

    private static readonly BuiltInFunction<Float1> InOutCubic = new(
        "float x",
        nameof(InOutCubic),
        "return x < 0.5 ? 4 * x * x * x : (x - 1) * (2 * x - 2) * (2 * x - 2) + 1;");

    private static readonly BuiltInFunction<Float1> InQuart = new(
        "float x",
        nameof(InQuart),
        "return x * x * x * x;");

    private static readonly BuiltInFunction<Float1> OutQuart = new(
        "float x",
        nameof(OutQuart),
        "return 1 - pow(1 - x, 4);");

    private static readonly BuiltInFunction<Float1> InOutQuart = new(
        "float x",
        nameof(InOutQuart),
        "return x < 0.5 ? 8 * x * x * x * x : 1 - pow(-2 * x + 2, 4) * 0.5;");

    private static readonly BuiltInFunction<Float1> InQuint = new(
        "float x",
        nameof(InQuint),
        "return x * x * x * x * x;");

    private static readonly BuiltInFunction<Float1> OutQuint = new(
        "float x",
        nameof(OutQuint),
        "return 1 + pow(x - 1, 5);");

    private static readonly BuiltInFunction<Float1> InOutQuint = new(
        "float x",
        nameof(InOutQuint),
        "return x < 0.5 ? 16 * x * x * x * x * x : 1 + pow(2 * x - 2, 5) * 0.5;");

    private static readonly BuiltInFunction<Float1> InExpo = new(
        "float x",
        nameof(InExpo),
        "return x == 0 ? 0 : pow(2, 10 * x - 10);");

    private static readonly BuiltInFunction<Float1> OutExpo = new(
        "float x",
        nameof(OutExpo),
        "return x == 1 ? 1 : 1 - pow(2, -10 * x);");

    private static readonly BuiltInFunction<Float1> InOutExpo = new(
        "float x",
        nameof(InOutExpo),
        "return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? pow(2, 20 * x - 10) * 0.5 : (2 - pow(2, -20 * x + 10)) * 0.5;");

    private static readonly BuiltInFunction<Float1> InCirc = new(
        "float x",
        nameof(InCirc),
        "return 1 - sqrt(1 - x * x);");

    private static readonly BuiltInFunction<Float1> OutCirc = new(
        "float x",
        nameof(OutCirc),
        "return sqrt(1 - pow(x - 1, 2));");

    private static readonly BuiltInFunction<Float1> InOutCirc = new(
        "float x",
        nameof(InOutCirc),
        "return x < 0.5 ? (1 - sqrt(1 - 4 * x * x)) * 0.5 : (sqrt(-((2 * x - 3) * (2 * x - 1)) + 1) + 1) * 0.5;");

    private static readonly BuiltInFunction<Float1> InBack = new(
        "float x",
        nameof(InBack),
        """
        float c1 = 1.70158;
        float c3 = c1 + 1;

        return c3 * x * x * x - c1 * x * x;
        """);

    private static readonly BuiltInFunction<Float1> OutBack = new(
        "float x",
        nameof(OutBack),
        """
        float c1 = 1.70158;
        float c3 = c1 + 1;

        return 1 + c3 * pow(x - 1, 3) + c1 * pow(x - 1, 2);
        """);

    private static readonly BuiltInFunction<Float1> InOutBack = new(
        "float x",
        nameof(InOutBack),
        """
        float c1 = 1.70158;
        float c2 = c1 * 1.525;

        return x < 0.5 ? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) * 0.5 : (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) * 0.5;
        """);

    private static readonly BuiltInFunction<Float1> InElastic = new(
        "float x",
        nameof(InElastic),
        $"""
         float c4 = (2 * {Pi}) / 3;

         return x == 0 ? 0 : x == 1 ? 1 : -pow(2, 10 * x - 10) * sin((x * 10 - 10.75) * c4);
         """);

    private static readonly BuiltInFunction<Float1> OutElastic = new(
        "float x",
        nameof(OutElastic),
        $"""
         float c4 = (2 * {Pi}) / 3;

         return x == 0 ? 0 : x == 1 ? 1 : pow(2, -10 * x) * sin((x * 10 - 0.75) * c4) + 1;
         """);

    private static readonly BuiltInFunction<Float1> InOutElastic = new(
        "float x",
        nameof(InOutElastic),
        $"""
         float c5 = (2 * {Pi}) / 4.5;

         return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? -(pow(2, 20 * x - 10) * sin((20 * x - 11.125) * c5)) * 0.5 : pow(2, -20 * x + 10) * sin((20 * x - 11.125) * c5) * 0.5 + 1;
         """);

    private static readonly BuiltInFunction<Float1> OutBounce = new(
        "float x",
        nameof(OutBounce),
        """
        float n1 = 7.5625;
        float d1 = 2.75;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5 / d1) * x + 0.75;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25 / d1) * x + 0.9375;
        }
        else
        {
            return n1 * (x -= 2.625 / d1) * x + 0.984375;
        }
        """);

    private static readonly BuiltInFunction<Float1> InBounce = new(
        "float x",
        nameof(InBounce),
        """
        return 1 - OutBounce(1 - x);
        """, OutBounce);

    private static readonly BuiltInFunction<Float1> InOutBounce = new(
        "float x",
        nameof(InOutBounce),
        """
        return x < 0.5 ? (1 - OutBounce(1 - 2 * x)) * 0.5 : (1 + OutBounce(2 * x - 1)) * 0.5;
        """, OutBounce);
}
