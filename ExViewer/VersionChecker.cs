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
        public static IAsyncOperation<bool> CheckAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var currentVersion = Package.Current.Id.Version;
                try
                {
                    using(var filter = new HttpBaseProtocolFilter())
                    {
                        filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                        filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                        using(var client = new HttpClient(filter))
                        {
                            var r = await client.GetAsync(new Uri("https://github.com/OpportunityLiu/ExViewer/releases/latest"));
                            var version = new string(r.RequestMessage.RequestUri.Segments.Last().Select(c => char.IsDigit(c) ? c : ' ').ToArray());
                            var subver = version.Split(default(char[]),  StringSplitOptions.RemoveEmptyEntries).Select(s => ushort.Parse(s)).ToArray();
                            if(subver.Length != 3)
                                return false;
                            if(subver[0] < currentVersion.Major)
                                return true;
                            if(subver[1] < currentVersion.Minor)
                                return true;
                            if(subver[2] < currentVersion.Build)
                                return true;
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
