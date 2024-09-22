using DrawiEngine;
using DrawiEngine.Browser;
using DrawieSample;

DrawingEngine engine = BrowserDrawingEngine.CreateDefaultBrowser();

DrawieSampleApp sampleApp = new DrawieSampleApp();

engine.RunWithApp(sampleApp);
