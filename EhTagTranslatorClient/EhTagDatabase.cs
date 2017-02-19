using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Namespace = ExClient.Namespace;

namespace EhTagTranslatorClient
{
    public static class EhTagDatabase
    {
        private static readonly Uri wikiDbRootUri = new Uri("ms-appx:///EhTagTranslatorClient/Data/");

        private static async Task<IReadOnlyDictionary<string, Record>> loadDatabaseTableAsync(Namespace @namespace)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{@namespace.ToString().ToLowerInvariant()}.md");
            var file = await StorageFile.GetFileFromApplicationUriAsync(dbUri);
            return Record.Analyze(await file.OpenSequentialReadAsync(), @namespace).ToDictionary(record => record.Original);
        }

        public static IAsyncOperation<IReadOnlyDictionary<Namespace, IReadOnlyDictionary<string, Record>>> LoadDatabaseAsync()
        {
            return Task.Run<IReadOnlyDictionary<Namespace, IReadOnlyDictionary<string, Record>>>(async () =>
            {
                var l = new Dictionary<Namespace, IReadOnlyDictionary<string, Record>>();
                var t = new List<Task<IReadOnlyDictionary<string, Record>>>();
                foreach(Namespace item in Enum.GetValues(typeof(Namespace)))
                {
                    t.Add(loadDatabaseTableAsync(item));
                }
                await Task.WhenAll(t);
                foreach(var item in t)
                {
                    var result = item.Result;
                    l.Add(result.Values.First().Namespace, result);
                }
                return l;
            }).AsAsyncOperation();
        }
    }
}
