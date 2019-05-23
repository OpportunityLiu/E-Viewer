namespace ExClient.Launch
{
    internal class WatchedHandler : SearchHandlerBase
    {
        public static WatchedHandler Instance { get; } = new WatchedHandler();

        public override bool CanHandle(UriHandlerData data)
        {
            return data.Paths.Count == 1 && data.Path0 == "watched";
        }

        public override SearchLaunchResult Handle(UriHandlerData data)
        {
            var keyword = GetKeyword(data);
            var category = GetCategory(data);
            var advanced = GetAdvancedSearchOptions(data);
            var sr = Client.Current.SearchWatched(keyword, category, advanced);
            return new SearchLaunchResult(sr);
        }

    }
}
