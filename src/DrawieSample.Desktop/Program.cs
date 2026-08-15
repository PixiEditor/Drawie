using Drawie2Sample;
using DrawiEngine;
using DrawiEngine.Desktop;

DrawingEngine engine = DesktopDrawingEngine.CreateDefaultDesktop(true);

Drawie2SampleApp app = new Drawie2SampleApp();

engine.RunWithApp(app);

await engine.Dispose();