using HtmlAgilityPack;
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
        public readonly struct VersionInfo
        {
            public VersionInfo(PackageVersion version, string title, string content)
            {
                this.Version = version;
                this.Title = title;
                this.Content = content;
            }

            public PackageVersion Version { get; }
            public string Title { get; }
            public string Content { get; }
        }

        public static Uri ReleaseUri { get; } = new Uri("https://github.com/OpportunityLiu/ExViewer/releases/latest");

        public static IAsyncOperation<VersionInfo?> CheckAsync()
        {
            return AsyncInfo.Run<VersionInfo?>(async token =>
            {
                var currentVersion = Package.Current.Id.Version;
                using (var filter = new HttpBaseProtocolFilter())
                {
                    filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    using (var client = new HttpClient(filter))
                    {
                        var r = await client.GetAsync(ReleaseUri);
                        var version = new string(r.RequestMessage.RequestUri.Segments.Last().Select(c => char.IsDigit(c) ? c : ' ').ToArray());
                        var subver = version.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(s => ushort.Parse(s)).ToArray();
                        if (subver.Length != 3 && subver.Length != 4)
                            return null;
                        var newVersion = new PackageVersion
                        {
                            Major = subver[0],
                            Minor = subver[1],
                            Build = subver[2],
                            Revision = subver.Length == 4 ? subver[3] : (ushort)0
                        };
                        var doc = new HtmlDocument();
                        doc.LoadHtml(await r.Content.ReadAsStringAsync());

                        var releaseNode = doc.DocumentNode.Descendants("div").Where(n => n.HasClass("release-body")).SingleOrDefault();
                        if (releaseNode is null)
                            return null;

                        var title = HtmlEntity.DeEntitize(releaseNode.Descendants("h1").Where(n => n.HasClass("release-title")).SingleOrDefault()?.InnerText ?? "").Trim();
                        var content = HtmlEntity.DeEntitize(releaseNode.Descendants("div").Where(n => n.HasClass("markdown-body")).SingleOrDefault()?.InnerText ?? "").Trim();
                        var newRelease = new VersionInfo(newVersion, title, content);

                        if (newVersion.Major > currentVersion.Major)
                            return newRelease;
                        else if (newVersion.Major < currentVersion.Major)
                            return null;
                        if (newVersion.Minor > currentVersion.Minor)
                            return newRelease;
                        else if (newVersion.Minor < currentVersion.Minor)
                            return null;
                        if (newVersion.Build > currentVersion.Build)
                            return newRelease;
                        return null;
                    }
                }
            });
        }
    }
}
