using EhTagClient.Models;
using ExClient;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
                try
                {
                    using(var client = new HttpClient())
                    {
                        var r = await client.GetStringAsync(new Uri("https://e-hentai.org/tools.php?act=taggroup"));
                        await updateDbAsync(r, token);
                        LastUpdate = DateTimeOffset.Now;
                    }

                }
                catch
                {

                    throw;
                }
            });
        }

        public static DataBase CreateDatabase() => new DataBase();

        private static async Task updateDbAsync(string s, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var html = new HtmlDocument();
            html.LoadHtml(s);
            var tab = html.DocumentNode.Element("html").Element("body").Element("table");
            var toAdd = new List<TagRecord>(15000);
            using(var db = new TagDb())
            {
                foreach(var item in tab.Elements("tr"))
                {
                    var link = item.Element("td")?.Element("a");
                    if(link == null)
                        continue;
                    var uri = link.GetAttributeValue("href", "");
                    var tagid = int.Parse(uri.Split('=').Last());
                    var tagstr = HtmlEntity.DeEntitize(link.InnerText);
                    var tag = Tag.Parse(tagstr);
                    var tagrecord = new TagRecord { TagConetnt = tag.Content, TagNamespace = tag.Namespace, TagId = tagid };
                    toAdd.Add(tagrecord);
                }
                token.ThrowIfCancellationRequested();
                db.TagTable.RemoveRange(db.TagTable);
                await db.SaveChangesAsync();
                await db.TagTable.AddRangeAsync(toAdd);
                await db.SaveChangesAsync();
            }
        }
    }

    public class DataBase : IDisposable
    {
        internal DataBase()
        {
            this.db = new TagDb();
        }

        public IQueryable<TagRecord> Tags => this.db.TagTable;

        private TagDb db;

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if(!this.disposedValue)
            {
                if(disposing)
                {
                    this.db.Dispose();
                }
                this.db = null;
                this.disposedValue = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
