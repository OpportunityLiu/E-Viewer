using ExClient.Tagging;
using Opportunity.MvvmUniverse.AsyncHelpers;
using System;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class SearchUploaderAndTagHandler : UriHandler
    {
        private static readonly char[] trim = new[] { '$' };

        public override bool CanHandle(UriHandlerData data)
        {
            if (data.Paths.Count != 2)
                return false;
            switch (data.Path0)
            {
            case "tag":
                return Tag.TryParse(data.Paths[1].Unescape2().TrimEnd(trim), out var ignore);
            case "uploader":
                return true;
            default:
                return false;
            }
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var v = data.Paths[1].Unescape2().Trim();
            switch (data.Path0)
            {
            case "tag":
                return AsyncWrapper.CreateCompleted<LaunchResult>(new SearchLaunchResult(Tag.Parse(v.TrimEnd(trim)).Search()));
            case "uploader":
                var uploader = v.IndexOf(' ') >= 0 ? $"uploader:\"{v}\"" : $"uploader:{v}";
                return AsyncWrapper.CreateCompleted<LaunchResult>(new SearchLaunchResult(Client.Current.Search(uploader)));
            }
            return AsyncWrapper.CreateError<LaunchResult>(new NotSupportedException("Unsupported uri."));
        }
    }
}
