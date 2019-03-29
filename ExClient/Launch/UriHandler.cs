using System.Threading.Tasks;

namespace ExClient.Launch
{
    internal abstract class UriHandler
    {
        public abstract bool CanHandle(UriHandlerData data);
        public abstract Task<LaunchResult> HandleAsync(UriHandlerData data);
    }
}
