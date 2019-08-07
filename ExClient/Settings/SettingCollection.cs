using ExClient.Internal;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
            _LoadSettingsDic();
        }

        private readonly Dictionary<string, string> _Settings = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> RawSettings => _Settings;

        private const string CACHE_NAME = "SettingsCache";

        internal void StoreCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values[owner.Type + CACHE_NAME] = JsonConvert.SerializeObject(_Settings);
        }

        private void loadCache()
        {
            var storage = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("ExClient", Windows.Storage.ApplicationDataCreateDisposition.Always);
            storage.Values.TryGetValue(owner.Type + CACHE_NAME, out var r);
            var value = r + "";
            _Settings.Clear();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            JsonConvert.PopulateObject(value, _Settings);
        }

        private void updateSettingsDic(HtmlDocument doc)
        {
            var outer = doc.GetElementbyId("outer");
            if (outer is null)
                return;

            _Settings.Clear();
            foreach (var item in outer.Descendants("input").Concat(outer.Descendants("textarea")))
            {
                var name = item.GetAttribute("name", default(string));
                if (name is null || name.StartsWith("profile"))
                {
                    continue;
                }

                switch (item.GetAttribute("type", default(string)))
                {
                case "radio":
                    if (item.GetAttribute("checked", "") == "checked")
                    {
                        _Settings[name] = item.GetAttribute("value", "");
                    }

                    break;
                case "checkbox":
                    if (item.GetAttribute("checked", "") == "checked")
                    {
                        _Settings[name] = item.GetAttribute("value", "on");
                    }

                    break;
                //case "text":
                //case "hidden":
                //case "submit":
                //textarea
                default:
                    _Settings[name] = item.GetAttribute("value", item.GetInnerText());
                    break;
                }
            }
        }

        public async Task FetchAsync(CancellationToken token = default)
        {
            Client.Current.CheckLogOn();
            try
            {
                var getDoc = Client.Current.HttpClient.GetDocumentAsync(configUri);
                token.Register(getDoc.Cancel);
                var doc = await getDoc;
                updateSettingsDic(doc);
            }
            finally
            {
                _LoadSettingsDic();
            }
        }

        private void _LoadSettingsDic()
        {
            foreach (var item in items.Values)
            {
                item.DataChanged(_Settings);
            }
            StoreCache();
            OnPropertyReset();
        }

        public async Task SendAsync(CancellationToken token = default)
        {
            try
            {
                if (_Settings.Count == 0)
                    await FetchAsync();

                var postDic = new Dictionary<string, string>(_Settings);
                foreach (var item in items.Values)
                {
                    item.ApplyChanges(postDic);
                }
                var isSame = true;
                if (postDic.Count == _Settings.Count)
                {
                    foreach (var item in postDic)
                    {
                        if (_Settings.TryGetValue(item.Key, out var ov) && ov == item.Value)
                            continue;
                        else
                        {
                            isSame = false;
                            break;
                        }
                    }
                }
                else
                    isSame = false;

                if (!isSame)
                {
                    var postData = Client.Current.HttpClient.PostAsync(configUri, postDic);
                    token.Register(postData.Cancel);
                    var r = await postData;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(await r.Content.ReadAsStringAsync());
                    updateSettingsDic(doc);
                }
            }
            finally
            {
                _LoadSettingsDic();
            }
        }

        private readonly Dictionary<string, SettingProvider> items = new Dictionary<string, SettingProvider>
        {
            ["Default"] = new DefaultSettingProvider(),
            [nameof(ExcludedLanguages)] = new ExcludedLanguagesSettingProvider(),
            [nameof(ExcludedUploaders)] = new ExcludedUploadersSettingProvider(),
            [nameof(ExcludedTagNamespaces)] = new ExcludedTagNamespacesSettingProvider(),
            [nameof(FavoriteCategoryNames)] = new FavoriteCategoryNamesSettingProvider(),
        };

        private SettingProvider _GetProvider([System.Runtime.CompilerServices.CallerMemberName]string key = null)
        {
            return items[key];
        }

        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)_GetProvider();
        public ExcludedUploadersSettingProvider ExcludedUploaders => (ExcludedUploadersSettingProvider)_GetProvider();
        public Tagging.Namespace ExcludedTagNamespaces
        {
            get => ((ExcludedTagNamespacesSettingProvider)_GetProvider()).Value;
            set
            {
                ((ExcludedTagNamespacesSettingProvider)_GetProvider()).Value = value;
                OnPropertyChanged();
            }
        }

        public ImageSize ResampledImageSize
        {
            get => ((DefaultSettingProvider)_GetProvider("Default")).ResampledImageSize;
            set
            {
                ((DefaultSettingProvider)_GetProvider("Default")).ResampledImageSize = value;
                OnPropertyChanged();
            }
        }

        public CommentsOrder CommentsOrder
        {
            get => ((DefaultSettingProvider)_GetProvider("Default")).CommentsOrder;
            set
            {
                ((DefaultSettingProvider)_GetProvider("Default")).CommentsOrder = value;
                OnPropertyChanged();
            }
        }

        public FavoritesOrder FavoritesOrder
        {
            get => ((DefaultSettingProvider)_GetProvider("Default")).FavoritesOrder;
            set
            {
                ((DefaultSettingProvider)_GetProvider("Default")).FavoritesOrder = value;
                OnPropertyChanged();
            }
        }

        public int TagFilteringThreshold
        {
            get => ((DefaultSettingProvider)_GetProvider("Default")).TagFilteringThreshold;
            set
            {
                ((DefaultSettingProvider)_GetProvider("Default")).TagFilteringThreshold = value;
                OnPropertyChanged();
            }
        }

        public int TagWatchingThreshold
        {
            get => ((DefaultSettingProvider)_GetProvider("Default")).TagWatchingThreshold;
            set
            {
                ((DefaultSettingProvider)_GetProvider("Default")).TagWatchingThreshold = value;
                OnPropertyChanged();
            }
        }

        public FavoriteCategoryNamesSettingProvider FavoriteCategoryNames => (FavoriteCategoryNamesSettingProvider)_GetProvider();

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

                settings["wt"] = _Wt.ToString();
                settings["ft"] = _Ft.ToString();
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

                void setInt(ref int field, string key, int def)
                {
                    if (!settings.TryGetValue(key, out var value)
                        || !int.TryParse(value, out field))
                        field = def;
                }
                setInt(ref _Ft, "ft", 0);
                setInt(ref _Wt, "wt", 0);
            }
            private static int _Clamp(int value, int v1, int v2)
            {
                if (value < v1)
                    return v1;
                if (value > v2)
                    return v2;
                return value;
            }

            public ImageSize ResampledImageSize;

            public CommentsOrder CommentsOrder;

            public FavoritesOrder FavoritesOrder;

            private int _Ft, _Wt;
            public int TagFilteringThreshold
            {
                get => _Ft;
                set => _Ft = _Clamp(value, -9999, 0);
            }

            public int TagWatchingThreshold
            {
                get => _Wt;
                set => _Wt = _Clamp(value, 0, 9999);
            }
        }
    }
}
