﻿using EhTagClient.Models;
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
using Windows.Web.Http.Filters;

namespace EhTagClient
{
    public static class Client
    {
        static Client()
        {
            TagDb.Migrate();
        }

        private const string LAST_UPDATE = "EhTagClient.LastUpdate";

        private static readonly Regex reg = new Regex(@"<a href=""https://repo\.e-hentai\.org/tools\.php\?act=taggroup&amp;mastertag=(\d+)"">([^<]+)</a>", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly string DbUri = "https://repo.e-hentai.org/tools.php?act=taggroup&show={0}";

        public static DateTimeOffset LastUpdate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(LAST_UPDATE, out var r))
                {
                    return (DateTimeOffset)r;
                }

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
                        using (var c = new HttpBaseProtocolFilter())
                        {
                            c.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
                            c.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
                            using (var client = new HttpClient(c))
                            using (var db = new TagDb())
                            {
                                var htmlTasks = new Task<string>[11];
                                for (var i = 0; i < 11; i++)
                                {
                                    var uri = new Uri(string.Format(DbUri, i + 1));
                                    htmlTasks[i] = client.GetStringAsync(uri).AsTask();
                                }
                                var htmls = await Task.WhenAll(htmlTasks);
                                token.ThrowIfCancellationRequested();
                                db.TagTable.RemoveRange(db.TagTable);
                                await db.SaveChangesAsync(token);
                                foreach (var item in htmls)
                                {
                                    await updateDbAsync(db, item, token);
                                }
                            }
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

        private static async Task updateDbAsync(TagDb db, string html, CancellationToken token)
        {
            var matches = reg.Matches(html);
            var toAdd = new List<TagRecord>(matches.Count);
            foreach (var item in matches.Cast<Match>())
            {
                var tagid = int.Parse(item.Groups[1].Value);
                var tagstr = item.Groups[2].Value;
                var tag = Tag.Parse(tagstr);
                var tagrecord = new TagRecord { TagConetnt = tag.Content, TagNamespace = tag.Namespace, TagId = tagid };
                toAdd.Add(tagrecord);
            }
            await db.TagTable.AddRangeAsync(toAdd, token);
            await db.SaveChangesAsync(token);
        }
    }
}
