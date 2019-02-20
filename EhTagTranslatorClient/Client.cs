using EhTagTranslatorClient.Model;
using ExClient.Tagging;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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

        private const string CURRENT_VERSION = "EhTagTranslatorClient.CurrentVersion";

        public static long CurrentVersion
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(CURRENT_VERSION, out var r))
                    return (long)r;
                return -1;
            }
            private set => ApplicationData.Current.LocalSettings.Values[CURRENT_VERSION] = value;
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

        private static readonly Uri _ReleaseApiUri = new Uri("https://api.github.com/repos/ehtagtranslation/Database/releases/latest");

        private static readonly string _ReleaseFileName = "db.text.json.gz";

        private static HttpClient _HttpClient;

        private static HttpClient HttpClient => LazyInitializer.EnsureInitialized(ref _HttpClient, () =>
        {
            var c = new HttpBaseProtocolFilter
            {
                CacheControl =
                {
                    ReadBehavior = HttpCacheReadBehavior.NoCache,
                    WriteBehavior = HttpCacheWriteBehavior.NoCache,
                }
            };
            return new HttpClient(c)
            {
                DefaultRequestHeaders =
                {
                    UserAgent =
                    {
                        new Windows.Web.Http.Headers.HttpProductInfoHeaderValue("EhTagTranslatorClient")
                    }
                }
            };
        });

        private static async Task<_Release> _GetReleaseAsync()
            => JsonConvert.DeserializeObject<_Release>(await HttpClient.GetStringAsync(_ReleaseApiUri));

        public static IAsyncOperation<bool> NeedUpdateAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var release = await _GetReleaseAsync();
                return release.id != CurrentVersion;
            });
        }

        public static IAsyncAction UpdateAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var release = await _GetReleaseAsync();
                var fileUri = release.assets.First(a => _ReleaseFileName.Equals(a.name, StringComparison.OrdinalIgnoreCase)).browser_download_url;
                using (var rawStream = await HttpClient.GetInputStreamAsync(fileUri))
                using (var stream = new GZipStream(rawStream.AsStreamForRead(), CompressionMode.Decompress))
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                using (var db = CreateDatabase())
                {
                    var data = (_ReleaseData)JsonSerializer.CreateDefault().Deserialize(reader, typeof(_ReleaseData));
                    db.Db.Table.RemoveRange(db.Db.Table);
                    await db.Db.SaveChangesAsync();
                    foreach (var item in data.data)
                    {
                        if (!Enum.TryParse<Namespace>(item.@namespace, true, out var ns))
                            continue;

                        foreach (var tag in item.data)
                        {
                            tag.Value.Namespace = ns;
                            tag.Value.Original = tag.Key;
                        }
                        db.Db.Table.AddRange(item.data.Values);
                        await db.Db.SaveChangesAsync();
                    }
                }

                CurrentVersion = release.id;
                LastUpdate = DateTimeOffset.Now;
            });
        }

#pragma warning disable IDE1006 // 命名样式
        private sealed class _Release
        {
            public long id { get; set; }
            public _Asset[] assets { get; set; }
        }

        private sealed class _Asset
        {
            public int id { get; set; }
            public string name { get; set; }
            public Uri browser_download_url { get; set; }
        }

        private sealed class _ReleaseData
        {
            public _RecordTable[] data { get; set; }
        }

        private sealed class _RecordTable
        {
            public string @namespace { get; set; }

            public IDictionary<string, Record> data { get; set; }
        }
#pragma warning restore IDE1006 // 命名样式
    }
}
