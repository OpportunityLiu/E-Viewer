using ExClient.Internal;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.Settings
{
    public enum ImageSize
    {
        Auto = 0,
        H780 = 1,
        H980 = 2,
        H1280 = 3,
        H1600 = 4,
        H2400 = 5,
    }

    public enum CommentsOrder
    {
        ByTimeAscending = 0,
        ByTimeDecending = 1,
        ByScore = 2,
    }

    public enum FavoritesOrder
    {
        ByLastUpdatedTime = 0,
        ByFavoritedTime = 1,
    }

    public sealed class SettingCollection : ObservableObject
    {
        private static readonly Uri configUriRaletive = new Uri("/uconfig.php", UriKind.Relative);
        private readonly Uri configUri;
        private readonly DomainProvider owner;

        internal SettingCollection(DomainProvider domain)
        {
            owner = domain;
            configUri = new Uri(domain.RootUri, configUriRaletive);
            foreach (var item in items.Values)
            {
                item.Owner = this;
            }
            loadCache();
            loadSettingsDic();
        }

        private readonly Dictionary<string, string> settings = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> RawSettings => settings;

        private const string CACHE_NAME = "SettingsCache";

        internal void StoreCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values[owner.Type + CACHE_NAME] = JsonConvert.SerializeObject(settings);
        }

        private void loadCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values.TryGetValue(owner.Type + CACHE_NAME, out var r);
            var value = r + "";
            settings.Clear();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            JsonConvert.PopulateObject(value, settings);
        }

        private void updateSettingsDic(HtmlDocument doc)
        {
            if (doc.ParseErrors.Any())
            {
                var html = doc.ParsedText;
                html = html.Replace("<td></div></td>", ""); // HOTFIX: HTML1509: 不匹配的结束标记。
                doc.LoadHtml(html);
            }
            var settings = doc.GetElementbyId("settings_outer");
            if (settings is null)
            {
                return;
            }

            this.settings.Clear();
            foreach (var item in settings.Descendants("input").Concat(settings.Descendants("textarea")))
            {
                var name = item.GetAttribute("name", default(string));
                if (name is null)
                {
                    continue;
                }

                switch (item.GetAttribute("type", default(string)))
                {
                case "radio":
                    if (item.GetAttribute("checked", "") == "checked")
                    {
                        this.settings[name] = item.GetAttribute("value", "");
                    }

                    break;
                case "checkbox":
                    if (item.GetAttribute("checked", "") == "checked")
                    {
                        this.settings[name] = item.GetAttribute("value", "on");
                    }

                    break;
                //case "text":
                //case "hidden":
                //case "submit":
                //textarea
                default:
                    this.settings[name] = item.GetAttribute("value", item.GetInnerText());
                    break;
                }
            }
        }

        public IAsyncAction FetchAsync()
        {
            Client.Current.CheckLogOn();
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    var getDoc = Client.Current.HttpClient.GetDocumentAsync(configUri);
                    token.Register(getDoc.Cancel);
                    var doc = await getDoc;
                    updateSettingsDic(doc);
                }
                finally
                {
                    loadSettingsDic();
                }
            });
        }

        private void loadSettingsDic()
        {
            foreach (var item in items.Values)
            {
                item.DataChanged(settings);
            }
            StoreCache();
            OnPropertyReset();
        }

        public IAsyncAction SendAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    if (settings.Count == 0)
                    {
                        await FetchAsync();
                    }

                    var postDic = new Dictionary<string, string>(settings);
                    foreach (var item in items.Values)
                    {
                        item.ApplyChanges(postDic);
                    }
                    var postData = Client.Current.HttpClient.PostAsync(configUri, postDic);
                    token.Register(postData.Cancel);
                    var r = await postData;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(await r.Content.ReadAsStringAsync());
                    updateSettingsDic(doc);
                }
                finally
                {
                    loadSettingsDic();
                }
            });
        }

        private readonly Dictionary<string, SettingProvider> items = new Dictionary<string, SettingProvider>
        {
            ["Default"] = new DefaultSettingProvider(),
            [nameof(ExcludedLanguages)] = new ExcludedLanguagesSettingProvider(),
            [nameof(ExcludedUploaders)] = new ExcludedUploadersSettingProvider(),
            [nameof(ExcludedTagNamespaces)] = new ExcludedTagNamespacesSettingProvider(),
            [nameof(FavoriteCategoryNames)] = new FavoriteCategoryNamesSettingProvider(),
        };

        private SettingProvider getProvider([System.Runtime.CompilerServices.CallerMemberName]string key = null)
        {
            return items[key];
        }

        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)getProvider();
        public ExcludedUploadersSettingProvider ExcludedUploaders => (ExcludedUploadersSettingProvider)getProvider();
        public Tagging.Namespace ExcludedTagNamespaces
        {
            get => ((ExcludedTagNamespacesSettingProvider)getProvider()).Value;
            set
            {
                ((ExcludedTagNamespacesSettingProvider)getProvider()).Value = value;
                OnPropertyChanged();
            }
        }

        public ImageSize ResampledImageSize
        {
            get => ((DefaultSettingProvider)getProvider("Default")).ResampledImageSize;
            set
            {
                ((DefaultSettingProvider)getProvider("Default")).ResampledImageSize = value;
                OnPropertyChanged();
            }
        }

        public CommentsOrder CommentsOrder
        {
            get => ((DefaultSettingProvider)getProvider("Default")).CommentsOrder;
            set => ((DefaultSettingProvider)getProvider("Default")).CommentsOrder = value;
        }

        public FavoritesOrder FavoritesOrder
        {
            get => ((DefaultSettingProvider)getProvider("Default")).FavoritesOrder;
            set => ((DefaultSettingProvider)getProvider("Default")).FavoritesOrder = value;
        }

        public FavoriteCategoryNamesSettingProvider FavoriteCategoryNames => (FavoriteCategoryNamesSettingProvider)getProvider();

        private sealed class DefaultSettingProvider : SettingProvider
        {
            internal override void ApplyChanges(Dictionary<string, string> settings)
            {
                // Original Images - Nope (Use local setting instead)
                settings["oi"] = "0";
                // Always use the Multi-Page Viewer - Nope 
                settings["qb"] = "0";
                // Thumbnail Size - Large
                settings["ts"] = "1";

                settings["xr"] = ((int)ResampledImageSize).ToString();
                settings["cs"] = ((int)CommentsOrder).ToString();
                settings["fs"] = ((int)FavoritesOrder).ToString();
            }

            internal override void DataChanged(Dictionary<string, string> settings)
            {
                void setEnum<T>(ref T field, string key, T def)
                    where T : struct, Enum
                {
                    if (!settings.TryGetValue(key, out var value)
                        || !Enum.TryParse<T>(value, true, out field))
                        field = def;
                }
                setEnum(ref ResampledImageSize, "xr", ImageSize.Auto);
                setEnum(ref CommentsOrder, "cs", CommentsOrder.ByTimeAscending);
                setEnum(ref FavoritesOrder, "fs", FavoritesOrder.ByLastUpdatedTime);
            }

            public ImageSize ResampledImageSize;

            public CommentsOrder CommentsOrder;

            public FavoritesOrder FavoritesOrder;
        }
    }
}
