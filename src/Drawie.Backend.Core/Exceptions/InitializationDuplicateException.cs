namespace Drawie.Backend.Core.Exceptions
{
    public class InitializationDuplicateException : Exception
    {
        public InitializationDuplicateException()
        {
        }

        public InitializationDuplicateException(string message) : base(message)
        {
        }
    }
}
