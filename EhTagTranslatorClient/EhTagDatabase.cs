using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using IRecordDictionary = System.Collections.Generic.IReadOnlyDictionary<string, EhTagTranslatorClient.Record>;
using Namespace = ExClient.Namespace;

namespace EhTagTranslatorClient
{
    public static class EhTagDatabase
    {
        private static readonly Uri wikiDbRootUri = new Uri("ms-appx:///EhTagTranslatorClient/Data/");

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

        public static IReadOnlyDictionary<Namespace, IRecordDictionary> Dictionary { get; private set; }

        private static async Task<IRecordDictionary> loadDatabaseTableAsync(Namespace @namespace)
        {
            var dbUri = new Uri(wikiDbRootUri, $"{@namespace.ToString().ToLowerInvariant()}.md");
            var file = await StorageFile.GetFileFromApplicationUriAsync(dbUri);
            using(var stream = await file.OpenSequentialReadAsync())
                return new ReadOnlyDictionary<string, Record>(Record.Analyze(stream, @namespace).ToDictionary(record => record.Original));
        }

        public static IAsyncAction LoadDatabaseAsync()
        {
            return Task.Run(async () =>
            {
                var l = new Dictionary<Namespace, IRecordDictionary>();
                foreach(var item in tables)
                {
                    var r = await loadDatabaseTableAsync(item);
                    l.Add(item, r);
                }
                Dictionary = new ReadOnlyDictionary<Namespace, IRecordDictionary>(l);
            }).AsAsyncAction();
        }
    }
}
