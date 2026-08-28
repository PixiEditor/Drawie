using Drawie2Sample;
using DrawiEngine;
using DrawiEngine.Browser;

public static class Program
{
    public static void Main()
    {
        DrawingEngine engine = BrowserDrawingEngine.CreateDefaultBrowser();

        Drawie2SampleApp sampleApp = new Drawie2SampleApp();

        engine.RunWithApp(sampleApp);
    }
}
