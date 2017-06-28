using EhTagTranslatorClient.Model;
using ExClient.Tagging;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace EhTagTranslatorClient
{
    public static class Client
    {
        static Client()
        {
            TranslateDb.Migrate();
        }

        public static DataBase CreateDatabase() => new DataBase();

        public static Record Get(Tag tag)
        {
            var ns = tag.Namespace;
            var key = tag.Content;
            using (var db = new TranslateDb())
            {
                return db.Table.AsNoTracking()
                    .SingleOrDefault(r => r.Namespace == ns && r.Original == key);
            }
        }

        private const string LAST_UPDATE = "EhTagTranslatorClient.LastUpdate";

        public static DateTimeOffset LastUpdate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_UPDATE, out var r))
                    return (DateTimeOffset)r;
                return DateTimeOffset.MinValue;
            }
            private set => ApplicationData.Current.LocalSettings.Values[LAST_UPDATE] = value;
        }

        private const string LAST_COMMIT = "EhTagTranslatorClient.LastCommit";

        public static DateTimeOffset LastCommit
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_COMMIT, out var r))
                    return (DateTimeOffset)r;
                return DateTimeOffset.MinValue.AddDays(1);
            }
            private set => ApplicationData.Current.LocalSettings.Values[LAST_COMMIT] = value;
        }

        private static readonly Uri wikiDbRootUri = new Uri("https://raw.githubusercontent.com/wiki/Mapaler/EhTagTranslator/tags/");

        private static readonly Uri stateUri = new Uri("https://github.com/Mapaler/EhTagTranslator/wiki/_history");

        public static IAsyncOperation<bool> NeedUpdateAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    using (var c = new HttpBaseProtocolFilter())
                    {
                        c.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                        c.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                        var client = new HttpClient(c);
                        var html = await client.GetStringAsync(stateUri);
                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);
                        var tr = doc.DocumentNode.Descendants("tr").First();
                        var rtime = tr.Descendants("relative-time").First();
                        var time = rtime.GetAttributeValue("datetime", "");
                        var dt = DateTimeOffset.Parse(time);
                        LastCommit = dt;
                    }
                }
                catch
                {
                }
                return LastCommit > LastUpdate;
            });
        }

        private static readonly Namespace[] tables = new[]
        {
            Namespace.Reclass,
            Namespace.Language,
            Namespace.Parody,
            Namespace.Character,
            Namespace.Group,
            Namespace.Artist,
            Namespace.Male,
            Namespace.Female,
            Namespace.Misc
        };

        private static IList<Record> analyzeDatabaseTableAsync(Namespace @namespace, IInputStream stream)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{@namespace.ToString().ToLowerInvariant()}.md");
            using (stream)
            {
                return Record.Analyze(stream, @namespace).ToList();
            }
        }

        public static IAsyncAction UpdateAsync()
        {
            return AsyncInfo.Run(async token => await Task.Run(async () =>
            {
                var streams = new IAsyncOperationWithProgress<IInputStream, HttpProgress>[tables.Length];
                var mergedCache = new Dictionary<string, Record>[tables.Length];

                using (var c = new HttpBaseProtocolFilter())
                {
                    c.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                    c.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                    var client = new HttpClient(c);
                    for (var i = 0; i < tables.Length; i++)
                    {
                        var dbUri = new Uri(wikiDbRootUri, $"{tables[i].ToString().ToLowerInvariant()}.md");
                        streams[i] = client.GetInputStreamAsync(dbUri);
                    }
                    for (var i = 0; i < tables.Length; i++)
                    {
                        using (var stream = await streams[i])
                        {
                            var unmergedCache = Record.Analyze(stream, tables[i]).ToList();
                            var dic = new Dictionary<string, Record>();
                            foreach (var item in unmergedCache)
                            {
                                if (dic.TryGetValue(item.Original, out var existed))
                                {
#if DEBUG
                                    if (System.Diagnostics.Debugger.IsAttached)
                                        System.Diagnostics.Debugger.Break();
#endif
                                    existed = Record.Combine(existed, item);
                                }
                                else
                                {
                                    existed = item;
                                }
                                dic[item.Original] = existed;
                            }
                            mergedCache[i] = dic;
                        }
                    }
                }
                using (var db = new TranslateDb())
                {
                    db.Table.RemoveRange(db.Table);
                    await db.SaveChangesAsync();
                    foreach (var item in mergedCache)
                    {
                        db.Table.AddRange(item.Values);
                        await db.SaveChangesAsync();
                    }
                }
                LastUpdate = DateTimeOffset.Now;
            }));
        }
    }
}
