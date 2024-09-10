using Drawie.Core;
using Drawie.Core.Bridge;
using Drawie.Core.Bridge.NativeObjectsImpl;
using Drawie.Core.Bridge.Operations;
using Drawie.Core.Surfaces;
using Drawie.RenderApi;
using Drawie.Skia.Exceptions;
using Drawie.Skia.Implementations;
using PixiEditor.Numerics;
using SkiaSharp;

namespace Drawie.Skia
{
    public class SkiaDrawingBackend : IDrawingBackend
    {
        private IVulkanRenderApi? vulkanRenderApi;
        
        public GRContext? GraphicsContext
        {
            get => _grContext;
            private set
            {
                if (_grContext != null)
                {
                    throw new GrContextAlreadyInitializedException();
                }
                
                _grContext = value;
            }
        }
        
        public bool IsHardwareAccelerated => GraphicsContext != null;
        
        public IRenderingDispatcher RenderingDispatcher { get; set; }

        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        ISurfaceImplementation IDrawingBackend.SurfaceImplementation => SurfaceImplementation;
        public SkiaSurfaceImplementation SurfaceImplementation { get; }
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        public IColorFilterImplementation ColorFilterImplementation { get; }
        public IImageFilterImplementation ImageFilterImplementation { get; }
        public IShaderImplementation ShaderImplementation { get; set; }

        private GRContext _grContext;

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;

            SkiaImageFilterImplementation imageFilterImpl = new SkiaImageFilterImplementation();
            ImageFilterImplementation = imageFilterImpl;
            
            SkiaShaderImplementation shader = new SkiaShaderImplementation();
            ShaderImplementation = shader;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl, imageFilterImpl, shader);
            PaintImplementation = paintImpl;
            
            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;
            
            MatrixImplementation = new SkiaMatrixImplementation();
            
            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl, pixmapImpl, shader);
            ImageImplementation = imgImpl;
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl, pixmapImpl);
            BitmapImplementation = bitmapImpl;
            
            SkiaCanvasImplementation canvasImpl = new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl);
            
            SurfaceImplementation = new SkiaSurfaceImplementation(GraphicsContext, pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImplementation(SurfaceImplementation);
            imgImpl.SetSurfaceImplementation(SurfaceImplementation);

            CanvasImplementation = canvasImpl;
        }
        
        public void Setup(IRenderApi renderApi)
        {
            // skia doesn't support webgpu :(
            
            if(renderApi is not IVulkanRenderApi vulkanRenderApi)
            {
                throw new UnsupportedRenderApiException(renderApi);
            }
            
            SetGraphicsContext(vulkanRenderApi);
            
            SurfaceImplementation.GrContext = GraphicsContext;
        }

        public DrawingSurface CreateRenderSurface(VecI size, IWindowRenderApi windowRenderApi)
        {
            if (windowRenderApi is IVulkanWindowRenderApi vkRenderApi)
            {
                var imageInfo = new GRVkImageInfo()
                {
                    CurrentQueueFamily = 0,
                    Format = vkRenderApi.RenderTexture.ImageFormat,
                    Image = vkRenderApi.RenderTexture.ImageHandle,
                    ImageLayout = vkRenderApi.RenderTexture.Layout, 
                    ImageTiling = vkRenderApi.RenderTexture.Tiling, 
                    ImageUsageFlags = vkRenderApi.RenderTexture.UsageFlags, 
                    LevelCount = 1,
                    SampleCount = 1,
                    Protected = false,
                    SharingMode = vkRenderApi.RenderTexture.TargetSharingMode, 
                };

                var surface = SKSurface.Create(GraphicsContext, new GRBackendRenderTarget(size.X, size.Y, 1, imageInfo),
                    GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888,
                    new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
                
                vkRenderApi.RenderTexture.MakeReadOnly();

                return DrawingSurface.FromNative(surface);
            }
         
            throw new RenderApiNotInitializedException();
        }
        
        private void SetGraphicsContext(IVulkanRenderApi vulkanRenderApi)
        {
            var windowRenderApi = vulkanRenderApi.WindowRenderApis.First();
            
            var vkBackendContext = new GRVkBackendContext()
            {
                VkDevice = windowRenderApi.LogicalDeviceHandle,
                VkInstance = windowRenderApi.InstanceHandle,
                VkPhysicalDevice = windowRenderApi.PhysicalDeviceHandle,
                VkQueue = windowRenderApi.GraphicsQueueHandle,
                GraphicsQueueIndex = 0,
                GetProcedureAddress = windowRenderApi.GetProcedureAddress
            };
            
            GraphicsContext = GRContext.CreateVulkan(vkBackendContext);
        }

        public override string ToString()
        {
            return "Skia";
        }
    }
}
