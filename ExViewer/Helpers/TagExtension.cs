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
        public static string GetDisplayContent(this Tag tag)
        {
            var r = GetDisplayContentAsync(tag);
            if (r.Status == AsyncStatus.Completed)
                return r.GetResults();
            return tag.Content;
        }

        public static IAsyncOperation<string> GetDisplayContentAsync(this Tag tag)
        {
            var settings = SettingCollection.Current;
            if (settings.UseChineseTagTranslation)
            {
                var r = tag.GetEhTagTranslatorRecord();
                if (r != null)
                    return new AsyncWrapper<string>(r.Translated.Text);
            }
            if (settings.UseJapaneseTagTranslation)
            {
                var t = tag.GetEhWikiRecordAsync();
                if (t.Status == AsyncStatus.Completed)
                {
                    var r = t.GetResults();
                    if (!match(tag, r))
                        return new AsyncWrapper<string>(tag.Content);
                    return new AsyncWrapper<string>(r.Japanese ?? tag.Content);
                }
                return Run(async token =>
                {
                    try
                    {
                        var r = await t;
                        if (!match(tag, r))
                            return tag.Content;
                        return r.Japanese ?? tag.Content;
                    }
                    catch (Exception)
                    {
                        return tag.Content;
                    }
                });
            }
            return new AsyncWrapper<string>(tag.Content);
        }

        private static bool match(Tag tag, EhWikiClient.Record wiki)
        {
            if (wiki == null)
                return false;
            if (tag.Namespace == Namespace.Unknown)
                return true;
            if (wiki.Type == EhWikiClient.TagType.Unknown)
                return true;
            switch (tag.Namespace)
            {
                case Namespace.Reclass:
                    return false;
                case Namespace.Language:
                    return wiki.Type.HasFlag(EhWikiClient.TagType.Language);
                case Namespace.Parody:
                    return wiki.Type.HasFlag(EhWikiClient.TagType.Series);
                case Namespace.Character:
                    return wiki.Type.HasFlag(EhWikiClient.TagType.Character);
                case Namespace.Group:
                case Namespace.Artist:
                    return wiki.Type.HasFlag(EhWikiClient.TagType.Creator);
                case Namespace.Male:
                case Namespace.Female:
                case Namespace.Misc:
                    return wiki.Type.HasFlag(EhWikiClient.TagType.Fetish);
                default:
                    return true;
            }
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
