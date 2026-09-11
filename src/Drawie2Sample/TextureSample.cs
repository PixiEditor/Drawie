using Drawie.Backend.Arco;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Rendering;
using Canvas = Drawie.Backend.Arco.Canvas;

public static class TextureSample
{
    static Canvas cnvs = null;

    public static void Draw(TextureFramebuffer target)
    {
        if (cnvs == null)
        {
            var device = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice;

            var texture1 = new Texture(new VecI(256, 256));
            var texture2 = new Texture(new VecI(256, 256));
            var texture3 = new Texture(new VecI(256, 256));
            var texture4 = new Texture(new VecI(256, 256));

            var paint = new Drawie.Backend.Core.Surfaces.PaintImpl.Paint() { IsAntiAliased = true};

            var canvas = texture1.DrawingSurface.Canvas;
            canvas.DrawColor(new Color(22, 28, 38), BlendMode.SrcOver);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    paint.Color = (x + y) % 2 == 0
                        ? new Color(70, 82, 100)
                        : new Color(35, 43, 56);

                    canvas.DrawRect(x * 32, y * 32, 32, 32, paint);
                }
            }

            paint.Color = new Color(255, 255, 255, 70);

            for (int i = 0; i < 9; i++)
                canvas.DrawCircle(16 + i * 30, 128, 8, paint);

            canvas = texture2.DrawingSurface.Canvas;
            canvas.DrawColor(new Color(18, 30, 48), BlendMode.SrcOver);

            for (int i = 0; i < 32; i++)
            {
                paint.Color = new Color(
                    (byte)(25 + i * 5),
                    (byte)(65 + i * 3),
                    (byte)(120 + i * 3));

                canvas.DrawRect(0, i * 8, 256, 8, paint);
            }

            paint.Color = new Color(255, 255, 255, 65);

            for (int i = -256; i < 512; i += 32)
                canvas.DrawRect(i, 0, 12, 256, paint);

            canvas = texture3.DrawingSurface.Canvas;
            canvas.DrawColor(new Color(42, 24, 45), BlendMode.SrcOver);

            for (int y = 0; y < 11; y++)
            {
                for (int x = 0; x < 11; x++)
                {
                    float cx = x * 25 + 12;
                    float cy = y * 25 + 12;
                    float radius = 4 + ((x * 17 + y * 31) % 9);

                    paint.Color = new Color(
                        (byte)(100 + (x * 17) % 120),
                        (byte)(45 + (y * 19) % 100),
                        (byte)(150 + (x * 11 + y * 7) % 100),
                        210);

                    canvas.DrawCircle(cx, cy, radius, paint);
                }
            }

            canvas = texture4.DrawingSurface.Canvas;
            canvas.DrawColor(new Color(14, 38, 34), BlendMode.SrcOver);

            float centerX = 128;
            float centerY = 128;

            for (int i = 0; i < 12; i++)
            {
                paint.Color = new Color(
                    (byte)(35 + i * 15),
                    (byte)(150 + i * 7),
                    (byte)(100 + i * 8),
                    220);

                canvas.DrawCircle(
                    centerX,
                    centerY,
                    15 + i * 9,
                    paint);
            }

            paint.Color = new Color(14, 38, 34);

            for (int i = 0; i < 6; i++)
            {
                canvas.DrawCircle(
                    centerX,
                    centerY,
                    8 + i * 8,
                    paint);
            }

            texture1.DrawingSurface.Canvas.Flush();
            texture2.DrawingSurface.Canvas.Flush();
            texture3.DrawingSurface.Canvas.Flush();
            texture4.DrawingSurface.Canvas.Flush();

            cnvs = new Canvas(device, target.Size);

            cnvs.DrawSurface(texture1, 40, 40, new Paint());
            cnvs.DrawSurface(texture2, 320, 40, new Paint());
            cnvs.DrawSurface(texture3, 40, 320, new Paint());
            cnvs.DrawSurface(texture4, 320, 320, new Paint());

            cnvs.Flush();
        }

        cnvs.BlitTo(target);
    }
}