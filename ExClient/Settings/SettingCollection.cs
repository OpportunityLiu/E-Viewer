using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Storage;
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

    public sealed class SettingCollection : ObservableObject
    {
        private readonly Client client;
        private static readonly Uri configUri = new Uri("/uconfig.php", UriKind.Relative);

        internal SettingCollection(Client client)
        {
            this.client = client;
            foreach (var item in this.items.Values)
            {
                item.Owner = this;
            }
            loadSettingsDic();
        }

        internal Dictionary<string, string> Settings => this.settingsCache.Value;

        private StorageProperty<Dictionary<string, string>> settingsCache = StorageProperty.CreateLocal("ExClient/SettingsCache", () => new Dictionary<string, string>());

        private void updateSettingsDic(HtmlDocument doc)
        {
            if (doc.ParseErrors.Any())
            {
                var html = doc.ParsedText;
                html = html.Replace("<td></div></td>", ""); // HOTFIX: HTML1509: 不匹配的结束标记。
                doc.LoadHtml(html);
            }
            var settings = doc.GetElementbyId("settings_outer");
            if (settings == null)
                return;
            Settings.Clear();
            foreach (var item in settings.Descendants("input").Concat(settings.Descendants("textarea")))
            {
                var name = item.GetAttribute("name", default(string));
                if (name == null)
                    continue;
                switch (item.GetAttribute("type", default(string)))
                {
                case "radio":
                    if (item.GetAttribute("checked", "") == "checked")
                        Settings[name] = item.GetAttribute("value", "");
                    break;
                case "checkbox":
                    if (item.GetAttribute("checked", "") == "checked")
                        Settings[name] = item.GetAttribute("value", "on");
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
                    var getDoc = this.client.HttpClient.GetDocumentAsync(configUri);
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
            this.settingsCache.Flush();
            OnPropertyChanged("");
        }

        public IAsyncAction SendAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                try
                {
                    if (this.Settings.Count == 0)
                        await FetchAsync();
                    var postDic = new Dictionary<string, string>(this.Settings);
                    foreach (var item in this.items.Values)
                    {
                        item.ApplyChanges(postDic);
                    }
                    var postData = this.client.HttpClient.PostAsync(configUri, new HttpFormUrlEncodedContent(postDic));
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
            [nameof(ExcludedTagNamespaces)] = new ExcludedTagNamespacesSettingProvider()
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
            set => ((ExcludedTagNamespacesSettingProvider)getProvider()).Value = value;
        }

        public ImageSize ResampledImageSize
        {
            get => ((DefaultSettingProvider)getProvider("Default")).ResampledImageSize;
            set => ((DefaultSettingProvider)getProvider("Default")).ResampledImageSize = value;
        }

        private sealed class DefaultSettingProvider : SettingProvider
        {
            internal override void ApplyChanges(Dictionary<string, string> settings)
            {
                // Thumbnail Size - Large
                settings["ts"] = "1";

                settings["xr"] = ((int)this.resampledImageSize).ToString();
            }

            internal override void DataChanged(Dictionary<string, string> settings)
            {
                this.resampledImageSize = (ImageSize)int.Parse(settings.GetValueOrDefault("xr", "0"));
            }

            private ImageSize resampledImageSize;
            public ImageSize ResampledImageSize
            {
                get => this.resampledImageSize;
                set => Set(ref this.resampledImageSize, value);
            }
        }
    }
}
