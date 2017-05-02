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
using Opportunity.MvvmUniverse.Helpers;

namespace ExClient
{
    static class TagExtension
    {
        public static IAsyncOperation<string> GetDisplayContentAsync(this Tag tag)
        {
            var settings = SettingCollection.Current;
            if(settings.UseChineseTagTranslation)
            {
                var r = tag.GetEhTagTranslatorRecord();
                if(r != null)
                    return new AsyncWrapper<string>(r.Translated.Text);
            }
            if(settings.UseJapaneseTagTranslation)
            {
                var t = tag.GetEhWikiRecordAsync();
                if(t.Status == AsyncStatus.Completed)
                {
                    var r = t.GetResults();
                    return new AsyncWrapper<string>(r?.Japanese ?? tag.Content);
                }
                return Run(async token =>
                {
                    try
                    {
                        var r = await t;
                        return r?.Japanese ?? tag.Content;
                    }
                    catch(Exception)
                    {
                        return tag.Content;
                    }
                });
            }
            return new AsyncWrapper<string>(tag.Content);
        }

        public static Record GetEhTagTranslatorRecord(this Tag tag)
        {
            return EhTagTranslatorClient.Client.Get(tag);
        }

        public static IAsyncOperation<EhWikiClient.Record> GetEhWikiRecordAsync(this Tag tag)
        {
            return EhWikiClient.Client.GetAsync(tag.Content);
        }

        public static IAsyncOperation<EhWikiClient.Record> FetchEhWikiRecordAsync(this Tag tag)
        {
            return EhWikiClient.Client.FetchAsync(tag.Content);
        }
    }
}
