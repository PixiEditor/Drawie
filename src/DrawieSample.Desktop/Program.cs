using DrawiEngine;
using DrawiEngine.Desktop;
using DrawieSample;

DrawingEngine engine = DesktopDrawingEngine.CreateDefaultDesktop();

DrawieSampleApp app = new DrawieSampleApp();

engine.RunWithApp(app);
