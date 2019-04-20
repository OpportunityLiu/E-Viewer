using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace ExViewer
{
    static class VersionChecker
    {
        public class User
        {
            public string login { get; set; }
            public int id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
        }

        public class Asset
        {
            public string url { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public string label { get; set; }
            public User uploader { get; set; }
            public string content_type { get; set; }
            public string state { get; set; }
            public int size { get; set; }
            public int download_count { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string browser_download_url { get; set; }
        }

        public sealed class GitHubRelease
        {
            public string url { get; set; }
            public string assets_url { get; set; }
            public string upload_url { get; set; }
            public string html_url { get; set; }
            public int id { get; set; }
            public string tag_name { get; set; }
            public string target_commitish { get; set; }
            public string name { get; set; }
            public bool draft { get; set; }
            public User author { get; set; }
            public bool prerelease { get; set; }
            public DateTime created_at { get; set; }
            public DateTime published_at { get; set; }
            public Asset[] assets { get; set; }
            public string tarball_url { get; set; }
            public string zipball_url { get; set; }
            public string body { get; set; }
            public PackageVersion Version { get; set; }
        }

        public static Uri ReleaseUri { get; } = new Uri("https://api.github.com/repos/OpportunityLiu/E-Viewer/releases/latest");

        public static IAsyncOperation<GitHubRelease> CheckAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                using (var filter = new HttpBaseProtocolFilter())
                {
                    filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    using (var client = new HttpClient(filter))
                    {
                        var currentVersion = Package.Current.Id.Version;
                        client.DefaultRequestHeaders.UserAgent.Add(new Windows.Web.Http.Headers.HttpProductInfoHeaderValue(Package.Current.Id.Name, currentVersion.ToVersion().ToString()));
                        var release = JsonConvert.DeserializeObject<GitHubRelease>(await client.GetStringAsync(ReleaseUri));
                        if (release is null || release.draft || release.prerelease)
                        {
                            return null;
                        }
                        var version = new string(release.tag_name.Select(c => char.IsDigit(c) ? c : ' ').ToArray());
                        var subver = version.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(s => ushort.Parse(s)).ToArray();
                        if (subver.Length != 3 && subver.Length != 4)
                        {
                            return null;
                        }
                        release.Version = new PackageVersion
                        {
                            Major = subver[0],
                            Minor = subver[1],
                            Build = subver[2],
                            Revision = subver.Length == 4 ? subver[3] : (ushort)0
                        };
                        if (release.Version.CompareTo(currentVersion) > 0)
                        {
                            return release;
                        }
                        return null;
                    }
                }
            });
        }
    }
}
