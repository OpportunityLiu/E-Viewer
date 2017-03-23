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
using EhWikiClient.Helpers;

namespace ExClient
{
    static class TagExtension
    {
        public static IAsyncAction Init()
        {
            return Task.Run(async () =>
            {
                var loadDb = EhTagDatabase.LoadDatabaseAsync();
                var loadWiki = EhWikiClient.Client.CreateAsync();
                await loadDb;
                await loadWiki;
                Application.Current.Suspending += App_Suspending;
            }).AsAsyncAction();
        }

        private static async void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var d = e.SuspendingOperation.GetDeferral();
            try
            {
                await EhWikiClient.Client.Instance.SaveAsync();
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
            if(settings.UseJapaneseTagTranslation && EhWikiClient.Client.Instance != null)
            {
                return Run(async token =>
                {
                    try
                    {
                        var r = await tag.GetEhWikiRecordAsync();
                        return r?.Japanese ?? tag.Content;
                    }
                    catch(Exception)
                    {
                        return tag.Content;
                    }
                });
            }
            return new AsyncWarpper<string>(tag.Content);
        }

        public static Record GetEhTagTranslatorRecord(this Tag tag)
        {
            if(EhTagDatabase.Dictionary == null)
                return null;
            var record = (Record)null;
            if(EhTagDatabase.Dictionary[tag.Namespace].TryGetValue(tag.Content, out record))
                return record;
            return null;
        }

        public static IAsyncOperation<EhWikiClient.Record> GetEhWikiRecordAsync(this Tag tag)
        {
            if(EhWikiClient.Client.Instance == null)
                return null;
            return EhWikiClient.Client.Instance.GetAsync(tag.Content);
        }

        public static IAsyncOperation<EhWikiClient.Record> FetchEhWikiRecordAsync(this Tag tag)
        {
            if(EhWikiClient.Client.Instance == null)
                return new AsyncWarpper<EhWikiClient.Record>();
            return EhWikiClient.Client.Instance.FetchAsync(tag.Content);
        }
    }
}
