using EhTagTranslatorClient;
using ExViewer.Settings;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Windows.Foundation;

namespace ExClient.Tagging
{
    static class TagExtension
    {
        public static string GetDisplayContent(this Tag tag)
        {
            var r = GetDisplayContentAsync(tag);
            if (r.Status == AsyncStatus.Completed)
            {
                return r.GetResults();
            }

            return tag.Content;
        }

        public static IAsyncOperation<string> GetDisplayContentAsync(this Tag tag)
        {
            var settings = SettingCollection.Current;
            if (settings.UseChineseTagTranslation)
            {
                var r = tag.GetEhTagTranslatorRecord();
                if (r != null)
                {
                    return AsyncOperation<string>.CreateCompleted(r.Translated);
                }
            }
            if (settings.UseJapaneseTagTranslation)
            {
                return tag.GetEhWikiRecordAsync().ContinueWith(action =>
                {
                    if (action.Status == AsyncStatus.Completed)
                    {
                        var r = action.GetResults();
                        if (!match(tag, r))
                        {
                            return tag.Content;
                        }
                        else
                        {
                            return r.Japanese ?? tag.Content;
                        }
                    }
                    else
                    {
                        throw action.ErrorCode;

                    }
                });
            }
            return AsyncOperation<string>.CreateCompleted(tag.Content);
        }

        private static bool match(Tag tag, EhWikiClient.Record wiki)
        {
            if (wiki is null)
            {
                return false;
            }

            if (tag.Namespace == Namespace.Unknown)
            {
                return true;
            }

            if (wiki.Type == EhWikiClient.TagType.Unknown)
            {
                return true;
            }

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
