using EhTagTranslatorClient.Model;
using ExClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using Namespace = ExClient.Namespace;

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
            using(var db = new TranslateDb())
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
                if(ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_UPDATE, out var r))
                    return (DateTimeOffset)r;
                return DateTimeOffset.MinValue;
            }
            private set => ApplicationData.Current.LocalSettings.Values[LAST_UPDATE] = value;
        }

        private static readonly Uri wikiDbRootUri = new Uri("https://raw.github.com/wiki/Mapaler/EhTagTranslator/tags/");

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

        private static async Task<IList<Record>> fetchDatabaseTableAsync(Namespace @namespace, HttpClient client)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{@namespace.ToString().ToLowerInvariant()}.md");
            using(var stream = await client.GetInputStreamAsync(dbUri))
            {
                return Record.Analyze(stream, @namespace).ToList();
            }
        }

        public static IAsyncAction UpdateAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var cache = new IList<Record>[tables.Length];
                using(var client = new HttpClient())
                {
                    for(var i = 0; i < tables.Length; i++)
                    {
                        cache[i] = await fetchDatabaseTableAsync(tables[i], client);
                        token.ThrowIfCancellationRequested();
                    }
                }
                using(var db = new TranslateDb())
                {
                    db.Table.RemoveRange(db.Table);
                    await db.SaveChangesAsync();
                    foreach(var item in cache)
                    {
                        db.Table.AddRange(item);
                    }
                    await db.SaveChangesAsync();
                }
                LastUpdate = DateTimeOffset.Now;
            });
        }
    }
}
