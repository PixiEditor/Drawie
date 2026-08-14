using Drawie2Sample;
using DrawiEngine;
using DrawiEngine.Desktop;

DrawingEngine engine = DesktopDrawingEngine.CreateDefaultDesktop();

Drawie2SampleApp app = new Drawie2SampleApp();

engine.RunWithApp(app);
