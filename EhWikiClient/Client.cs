using EhWikiClient.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace EhWikiClient
{
    public static class Client
    {
        static Client()
        {
            WikiDb.Migrate();
        }

        public static DataBase CreateDatabase() => new DataBase();

        private static HttpClient http = new HttpClient();

        public static Record Get(string title)
        {
            using(var db = new WikiDb())
            {
                var record = db.Table.AsNoTracking().SingleOrDefault(r => r.Title == title);
                if(record != null)
                    return record;
                FetchAsync(title).Completed = (s, e) =>
                {
                    if(e == AsyncStatus.Error)
                        s.ErrorCode.ToString();
                };
                return null;
            }
        }

        public static IAsyncOperation<Record> GetAsync(string title)
        {
            using(var db = new WikiDb())
            {
                var record = db.Table.AsNoTracking().SingleOrDefault(r => r.Title == title);
                if(record != null)
                    return Helpers.AsyncWarpper.Create(record);
                return FetchAsync(title);
            }
        }

        public static Uri WikiUri { get; } = new Uri("https://ehwiki.org/");

        private static readonly Uri apiUri = new Uri(WikiUri, "api.php");

        public static IAsyncOperation<Record> FetchAsync(string title)
        {
            return Run(async token =>
            {
                IEnumerable<KeyValuePair<string, string>> getRequestParameters()
                {
                    //https://ehwiki.org/api.php?action=parse&page={pageName}&prop=text&format=jsonfm&utf8=
                    yield return new KeyValuePair<string, string>("action", "parse");
                    yield return new KeyValuePair<string, string>("page", title);
                    yield return new KeyValuePair<string, string>("prop", "text");
                    yield return new KeyValuePair<string, string>("format", "json");
                    yield return new KeyValuePair<string, string>("utf8", "");
                }
                var post = http.PostAsync(apiUri, new HttpFormUrlEncodedContent(getRequestParameters()));
                token.Register(post.Cancel);
                var res = await post;
                var resStr = await res.Content.ReadAsStringAsync();
                var record = Record.Load(resStr);
                if(record == null)
                    return null;
                using(var db = new WikiDb())
                {
                    var oldrecord = db.Table.SingleOrDefault(r => r.Title == record.Title);
                    if(oldrecord == null)
                        db.Table.Add(record);
                    else
                    {
                        oldrecord.Japanese = record.Japanese;
                        oldrecord.Description = record.Description;
                    }
                    await db.SaveChangesAsync();
                }
                return record;
            });
        }
    }
}
