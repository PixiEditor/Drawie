using Drawie.Backend.Core.Exceptions;
using Drawie.RenderApi;

namespace Drawie.Backend.Core.Bridge
{
    public static class DrawingBackendApi
    {
        public static event Action OnBackendInitialized;

        private static IDrawingBackend? _current;

        public static IDrawingBackend Current
        {
            get
            {
                if (_current == null)
                    throw new NullReferenceException(
                        "Either drawing backend was not yet initialized or reference was somehow lost.");

                return _current;
            }
        }

        public static bool HasBackend => _current != null;

        public static void SetupBackend(IDrawingBackend backend, IRenderingDispatcher dispatcher)
        {
            if (_current != null)
            {
                throw new InitializationDuplicateException("Drawing backend was already initialized.");
            }

            _current = backend;
            _current.RenderingDispatcher = dispatcher;

            OnBackendInitialized?.Invoke();
        }

        public static void InitializeBackend(IRenderApi renderApi)
        {
            if (_current == null)
            {
                throw new NullReferenceException("Drawing backend was not yet initialized.");
            }

            _current.Setup(renderApi);
        }
    }
}
