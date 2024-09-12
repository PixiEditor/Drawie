using System;
using Drawie.Windowing.Browser;
using DrawiEngine;

DrawingEngine engine = DrawingEngine.CreateDefaultBrowser();

var window = engine.WindowingPlatform.CreateWindow("Drawie Browser Sample");

engine.RunWithWindow(window);
