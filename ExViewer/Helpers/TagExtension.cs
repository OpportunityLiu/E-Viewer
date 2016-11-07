using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExViewer.Settings;
using EhTagTranslatorClient;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.UI.Xaml;
using ExViewer;
using GalaSoft.MvvmLight.Threading;
using ExViewer.Helpers;

namespace ExClient
{
    static class TagExtension
    {
        private static IReadOnlyDictionary<NameSpace, IReadOnlyDictionary<string, Record>> tagDb;
        private static EhWikiClient.Client wikiClient;

        public static IAsyncAction Init()
        {
            return Task.Run(() =>
            {
                var loadDb = EhTagDatabase.LoadDatabaseAsync();
                loadDb.Completed = (sender, e) =>
                {
                    tagDb = sender.GetResults();
                };
                var loadWiki = EhWikiClient.Client.CreateAsync();
                loadWiki.Completed = (sender, e) =>
                {
                    wikiClient = sender.GetResults();
                };
                Application.Current.Suspending += App_Suspending;
            }).AsAsyncAction();
        }

        private static async void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var d = e.SuspendingOperation.GetDeferral();
            try
            {
                await wikiClient.SaveAsync();
            }
            finally
            {
                d.Complete();
            }
        }

        public static IAsyncOperation<string> GetDisplayContentAsync(this Tag tag)
        {
            var settings = SettingCollection.Current;
            if(settings.UseChineseTagTranslation)
            {
                var r = tag.GetEhTagTranslatorRecord();
                if(r != null)
                    return new AsyncWarpper<string>(r.Translated.Text);
            }
            if(settings.UseJapaneseTagTranslation && wikiClient != null)
            {
                return Run(async token =>
                {
                    var r = await tag.GetEhWikiRecordAsync();
                    return r.Japanese ?? tag.Content;
                });
            }
            return new AsyncWarpper<string>(tag.Content);
        }

        public static Record GetEhTagTranslatorRecord(this Tag tag)
        {
            if(tagDb == null)
                return null;
            var record = (Record)null;
            if(tagDb[tag.NameSpace].TryGetValue(tag.Content, out record))
                return record;
            return null;
        }

        public static IAsyncOperation<EhWikiClient.Record> GetEhWikiRecordAsync(this Tag tag)
        {
            if(wikiClient == null)
                return null;
            return wikiClient.GetAsync(tag.Content);
        }

        public static IAsyncOperation<EhWikiClient.Record> FetchEhWikiRecordAsync(this Tag tag)
        {
            if(wikiClient == null)
                return new AsyncWarpper<EhWikiClient.Record>();
            return wikiClient.FetchAsync(tag.Content);
        }
    }
}
