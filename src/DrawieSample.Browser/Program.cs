using DrawiEngine;
using DrawieSample;

DrawingEngine engine = DrawingEngine.CreateDefaultBrowser();

DrawieSampleApp sampleApp = new DrawieSampleApp();

engine.RunWithApp(sampleApp);
