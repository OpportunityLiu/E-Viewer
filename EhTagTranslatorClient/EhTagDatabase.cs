using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using NameSpace = ExClient.NameSpace;

namespace EhTagTranslatorClient
{
    public static class EhTagDatabase
    {
        private static readonly Uri wikiDbRootUri = new Uri("ms-appx:///EhTagTranslatorClient/Data/");

        private static async Task<IReadOnlyDictionary<string, Record>> loadDatabaseTableAsync(NameSpace nameSpace)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{nameSpace.ToString().ToLowerInvariant()}.md");
            var file = await StorageFile.GetFileFromApplicationUriAsync(dbUri);
            return Record.Analyze(await file.OpenSequentialReadAsync(), nameSpace).ToDictionary(record => record.Original);
        }

        public static IAsyncOperation<IReadOnlyDictionary<NameSpace, IReadOnlyDictionary<string, Record>>> LoadDatabaseAsync()
        {
            return Task.Run<IReadOnlyDictionary<NameSpace, IReadOnlyDictionary<string, Record>>>(async () =>
            {
                var l = new Dictionary<NameSpace, IReadOnlyDictionary<string, Record>>();
                var t = new List<Task<IReadOnlyDictionary<string, Record>>>();
                foreach(NameSpace item in Enum.GetValues(typeof(NameSpace)))
                {
                    t.Add(loadDatabaseTableAsync(item));
                }
                await Task.WhenAll(t);
                foreach(var item in t)
                {
                    var result = item.Result;
                    l.Add(result.Values.First().NameSpace, result);
                }
                return l;
            }).AsAsyncOperation();
        }
    }
}
