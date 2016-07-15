using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using NameSpace = ExClient.NameSpace;

namespace EhTagTranslatorClient
{
    internal class ContentFetcher : IDisposable
    {
        public static ContentFetcher Current
        {
            get;
        } = new ContentFetcher
        {
            client = new HttpClient()
        };

        public static ContentFetcher CurrentLocal
        {
            get;
        } = getCurrentLocal();

        private static ContentFetcher getCurrentLocal()
        {
            var f = new HttpBaseProtocolFilter();
            f.CacheControl.ReadBehavior = HttpCacheReadBehavior.OnlyFromCache;
            var c = new HttpClient(f);
            return new ContentFetcher
            {
                client = c
            };
        }

        private ContentFetcher() { }

        private HttpClient client;

        private static readonly Uri wikiRootUri = new Uri("https://raw.github.com/wiki/Mapaler/EhTagTranslator/");
        private static readonly Uri wikiRowsUri = new Uri(wikiRootUri, "rows.md");
        private static readonly Uri wikiDbRootUri = new Uri(wikiRootUri, "tags/");

        private static readonly Regex dbVersionRegex = new Regex(@"<\s*a\s+href\s*=\s*[""']ETB_wiki-version['""]\s*>\s*(?<version>\d+)\s*<\s*/\s*a\s*>", RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public IAsyncOperation<uint> FetchOnlineDatabaseVersionAsync()
        {
            return Run(async token =>
            {
                var getRows = client.GetStringAsync(wikiRowsUri);
                token.Register(getRows.Cancel);
                var rows = await getRows;
                var dbVersionMatch = dbVersionRegex.Match(rows);
                if(!dbVersionMatch.Success)
                    return EhTagDatabase.ClientVersion;
                return uint.Parse(dbVersionMatch.Groups["version"].Value);
            });
        }

        public async Task<IEnumerable<Record>> FetchDatabaseTableAsync(NameSpace nameSpace)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{nameSpace.ToString().ToLowerInvariant()}.md");
            var getDb = client.GetInputStreamAsync(dbUri);
            return Record.Analyze(await getDb, nameSpace);
        }

        public IAsyncOperation<IList<Record>> FetchDatabaseAsync()
        {
            return Task.Run(async () =>
            {
                var l = new List<Record>();
                var t = new List<Task<IEnumerable<Record>>>();
                foreach(NameSpace item in Enum.GetValues(typeof(NameSpace)))
                {
                    t.Add(FetchDatabaseTableAsync(item));
                }
                await Task.WhenAll(t);
                foreach(var item in t)
                {
                    l.AddRange(item.Result);
                }
                return (IList<Record>)l;
            }).AsAsyncOperation();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    client.Dispose();
                }
                client = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
