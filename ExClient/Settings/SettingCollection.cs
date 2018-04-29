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

        private readonly DomainProvider owner;
        private readonly Uri configUri;

        internal SettingCollection(DomainProvider owner)
        {
            this.owner = owner;
            this.configUri = new Uri(owner.RootUri, configUriRaletive);
            foreach (var item in this.items.Values)
            {
                item.Owner = this;
            }
            loadCache();
            loadSettingsDic();
        }

        internal Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();

        internal void StoreCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values[this.owner.Type + "SettingsCache"] = JsonConvert.SerializeObject(Settings);
        }

        private void loadCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values.TryGetValue(this.owner.Type + "SettingsCache", out var r);
            var value = r + "";
            Settings.Clear();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            JsonConvert.PopulateObject(value, Settings);
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

            Settings.Clear();
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
                        Settings[name] = item.GetAttribute("value", "");
                    }

                    break;
                case "checkbox":
                    if (item.GetAttribute("checked", "") == "checked")
                    {
                        Settings[name] = item.GetAttribute("value", "on");
                    }

                    break;
                //case "text":
                //case "hidden":
                //case "submit":
                //textarea
                default:
                    Settings[name] = item.GetAttribute("value", item.GetInnerText());
                    break;
                }
            }
        }

        public IAsyncAction FetchAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    var getDoc = Client.Current.HttpClient.GetDocumentAsync(this.configUri);
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
            foreach (var item in this.items.Values)
            {
                item.DataChanged(this.Settings);
            }
            StoreCache();
            OnPropertyChanged("");
        }

        public IAsyncAction SendAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    if (this.Settings.Count == 0)
                    {
                        await FetchAsync();
                    }

                    var postDic = new Dictionary<string, string>(this.Settings);
                    foreach (var item in this.items.Values)
                    {
                        item.ApplyChanges(postDic);
                    }
                    var postData = Client.Current.HttpClient.PostAsync(this.configUri, new HttpFormUrlEncodedContent(postDic));
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
            return this.items[key];
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
                // Thumbnail Size - Large
                settings["ts"] = "1";
                // Popular Right Now - Display
                if (this.Owner.owner.Type == HostType.EHentai)
                {
                    settings["pp"] = "0";
                }

                settings["xr"] = ((int)this.ResampledImageSize).ToString();
                settings["cs"] = ((int)this.CommentsOrder).ToString();
                settings["fs"] = ((int)this.FavoritesOrder).ToString();
            }

            internal override void DataChanged(Dictionary<string, string> settings)
            {
                if (settings.TryGetValue("xr", out var xr))
                {
                    this.ResampledImageSize = (ImageSize)int.Parse(xr);
                }
                else
                {
                    this.ResampledImageSize = ImageSize.Auto;
                }

                if (settings.TryGetValue("cs", out var cs))
                {
                    this.CommentsOrder = (CommentsOrder)int.Parse(cs);
                }
                else
                {
                    this.CommentsOrder = CommentsOrder.ByTimeAscending;
                }

                if (settings.TryGetValue("fs", out var fs))
                {
                    this.FavoritesOrder = (FavoritesOrder)int.Parse(fs);
                }
                else
                {
                    this.FavoritesOrder = FavoritesOrder.ByLastUpdatedTime;
                }
            }

            public ImageSize ResampledImageSize;

            public CommentsOrder CommentsOrder;

            public FavoritesOrder FavoritesOrder;
        }
    }
}
