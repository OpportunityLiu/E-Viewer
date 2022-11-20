using System.Threading.Tasks;

namespace ExClient.Launch
{
    internal sealed class PopularHandler : UriHandler
    {
        public override bool CanHandle(UriHandlerData data) => data.Path0 == "popular";

        public override Task<LaunchResult> HandleAsync(UriHandlerData data)
        {
            return Task.FromResult<LaunchResult>(PopularLaunchResult.Instance);
        }
    }
}
