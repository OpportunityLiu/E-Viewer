using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExViewer.Settings;
using EhTagTranslatorClient;
using Windows.Foundation;

namespace ExClient
{
    static class TagExtension
    {
        private static IList<Record> tagDb;

        public static IAsyncAction Init()
        {
            return Task.Run(() =>
            {
                var loadDb = EhTagDatabase.LoadDatabaseAsync();
                loadDb.Completed = (sender, e) =>
                {
                    tagDb = sender.GetResults();
                };
            }).AsAsyncAction();
        }

        public static string GetDisplayContent(this Tag tag)
        {
            if(!SettingCollection.Current.UseTagTranslation || tagDb == null)
                return tag.Content;
            var record = tagDb.SingleOrDefault(r => r.Original == tag.Content && r.NameSpace == tag.NameSpace);
            if(record == null)
                return tag.Content;
            else
                return record.Translated.Text;
        }
    }
}
