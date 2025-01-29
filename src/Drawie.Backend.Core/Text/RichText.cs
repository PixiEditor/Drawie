using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace Drawie.Backend.Core.Text;

public class RichText
{
    public string RawText { get; set; }

    public string FormattedText { get; }

    public string[] Lines { get; }
    public bool Fill { get; set; }
    public Color FillColor { get; set; }
    public float StrokeWidth { get; set; }
    public Color StrokeColor { get; set; }

    public RichText(string text)
    {
        RawText = text;
        FormattedText = text.Replace(Environment.NewLine, " ");
        Lines = text.Split(Environment.NewLine);
    }

    public void Paint(Canvas canvas, VecD position, Font font, Paint paint, VectorPath? onPath)
    {
        if (onPath != null)
        {
            canvas.DrawTextOnPath(onPath, FormattedText, position, font, paint);
        }
        else
        {
            VecD linePosition = position;
            foreach (var line in Lines)
            {
                bool hasStroke = StrokeWidth > 0;
                bool hasFill = Fill && FillColor.A > 0;
                bool strokeAndFillEqual = StrokeColor == FillColor;

                if (hasStroke && hasFill && strokeAndFillEqual)
                {
                    paint.Style = PaintStyle.StrokeAndFill;
                    paint.Color = StrokeColor;
                    paint.StrokeWidth = StrokeWidth;

                    PaintLine(canvas, line, linePosition, font, paint);
                }
                else
                {
                    if (hasStroke)
                    {
                        paint.Style = PaintStyle.Stroke;
                        paint.Color = StrokeColor;
                        paint.StrokeWidth = StrokeWidth;
                        PaintLine(canvas, line, linePosition, font, paint);
                    }

                    if (hasFill)
                    {
                        paint.Style = PaintStyle.Fill;
                        paint.Color = FillColor;
                        PaintLine(canvas, line, linePosition, font, paint);
                    }
                }

                paint.Style = PaintStyle.StrokeAndFill; // for measurements
                paint.StrokeWidth = 0;

                font.MeasureText(line, out RectD bounds, paint);

                linePosition.Y += bounds.Height;
            }
        }
    }

    private void PaintLine(Canvas canvas, string line, VecD position, Font font, Paint paint)
    {
        canvas.DrawText(line, position, font, paint);
    }

    public RectD MeasureBounds(Font font)
    {
        using Paint measurementPaint = new Paint();
        measurementPaint.Style = PaintStyle.StrokeAndFill;
        measurementPaint.StrokeWidth = StrokeWidth;

        double height = 0;
        double width = 0;
        double y = 0;
        double x = 0;

        foreach (var line in Lines)
        {
            font.MeasureText(line, out RectD bounds, measurementPaint);

            if (bounds.Width > width)
            {
                width = bounds.Width;
            }

            if (bounds.Y < y)
            {
                y = bounds.Y;
            }

            if (bounds.X < x)
            {
                x = bounds.X;
            }

            height += bounds.Height;
        }

        return new RectD(x, y, width, height);
    }
}
