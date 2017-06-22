using EhTagClient.Models;
using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;

namespace EhTagClient
{
    public static class Client
    {
        static Client()
        {
            TagDb.Migrate();
        }

        private const string LAST_UPDATE = "EhTagClient.LastUpdate";

        private static readonly Regex reg = new Regex(@"<a href=""https://e-hentai\.org/tools\.php\?act=taggroup&amp;taggroup=(\d+)"" style=""color:black"">([^<]+)</a>", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Uri DbUri = new Uri("https://e-hentai.org/tools.php?act=taggroup");

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

        public static IAsyncAction UpdateAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        using(var client = new HttpClient())
                        {
                            var r = await client.GetStringAsync(DbUri);
                            await updateDbAsync(r, token);
                        }
                        LastUpdate = DateTimeOffset.Now;
                    }
                    catch
                    {
                        throw;
                    }
                });
            });
        }

        public static DataBase CreateDatabase() => new DataBase();

        private static async Task updateDbAsync(string s, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var matches = reg.Matches(s);
            var toAdd = new List<TagRecord>(matches.Count);
            foreach(var item in matches.Cast<Match>())
            {
                var tagid = int.Parse(item.Groups[1].Value);
                var tagstr = item.Groups[2].Value;
                var tag = Tag.Parse(tagstr);
                var tagrecord = new TagRecord { TagConetnt = tag.Content, TagNamespace = tag.Namespace, TagId = tagid };
                toAdd.Add(tagrecord);
            }
            token.ThrowIfCancellationRequested();
            using(var db = new TagDb())
            {
                db.TagTable.RemoveRange(db.TagTable);
                await db.SaveChangesAsync();
                db.TagTable.AddRange(toAdd);
                await db.SaveChangesAsync();
            }
        }
    }
}
