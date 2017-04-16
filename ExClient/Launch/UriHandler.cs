using Windows.Foundation;

namespace ExClient.Launch
{
    internal abstract class UriHandler
    {
        public abstract bool CanHandle(UriHandlerData data);
        public abstract IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data);
    }
}
