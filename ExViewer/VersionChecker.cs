using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace ExViewer
{
    static class VersionChecker
    {
        public static Uri ReleaseUri { get; } = new Uri("https://github.com/OpportunityLiu/ExViewer/releases/latest");

        public static IAsyncOperation<PackageVersion?> CheckAsync()
        {
            return AsyncInfo.Run<PackageVersion?>(async token =>
            {
                var currentVersion = Package.Current.Id.Version;
                using(var filter = new HttpBaseProtocolFilter())
                {
                    filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    using(var client = new HttpClient(filter))
                    {
                        var r = await client.GetAsync(ReleaseUri);
                        var version = new string(r.RequestMessage.RequestUri.Segments.Last().Select(c => char.IsDigit(c) ? c : ' ').ToArray());
                        var subver = version.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries).Select(s => ushort.Parse(s)).ToArray();
                        if(subver.Length != 3)
                            return null;
                        var newVersion = new PackageVersion
                        {
                            Major = subver[0],
                            Minor = subver[1],
                            Build = subver[2],
                            Revision = 0
                        };
                        if(newVersion.Major > currentVersion.Major)
                            return newVersion;
                        else if(newVersion.Major < currentVersion.Major)
                            return null;
                        if(newVersion.Minor > currentVersion.Minor)
                            return newVersion;
                        else if(newVersion.Minor < currentVersion.Minor)
                            return null;
                        if(newVersion.Build > currentVersion.Build)
                            return newVersion;
                        return null;
                    }
                }
            });
        }
    }
}
