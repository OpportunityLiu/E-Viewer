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

        private static async Task<IEnumerable<Record>> loadDatabaseTableAsync(NameSpace nameSpace)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{nameSpace.ToString().ToLowerInvariant()}.md");
            var file = await StorageFile.GetFileFromApplicationUriAsync(dbUri);
            return Record.Analyze(await file.OpenSequentialReadAsync(), nameSpace);
        }

        public static IAsyncOperation<IList<Record>> LoadDatabaseAsync()
        {
            return Task.Run(async () =>
            {
                var l = new List<Record>();
                var t = new List<Task<IEnumerable<Record>>>();
                foreach(NameSpace item in Enum.GetValues(typeof(NameSpace)))
                {
                    t.Add(loadDatabaseTableAsync(item));
                }
                await Task.WhenAll(t);
                foreach(var item in t)
                {
                    l.AddRange(item.Result);
                }
                return (IList<Record>)l;
            }).AsAsyncOperation();
        }
    }
}
