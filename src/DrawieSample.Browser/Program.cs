using DrawiEngine;
using DrawiEngine.Browser;
using DrawieSample;

public static class Program
{
    public static void Main()
    {
        DrawingEngine engine = BrowserDrawingEngine.CreateDefaultBrowser();

        DrawieSampleApp sampleApp = new DrawieSampleApp();

        engine.RunWithApp(sampleApp);
    }
}
