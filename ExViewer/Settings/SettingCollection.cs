using ApplicationDataManager.Settings;
using ExClient;
using ExClient.Settings;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using ExViewer.Helpers;
using ExClient.Tagging;

namespace ExViewer.Settings
{
    public class SettingCollection : ApplicationSettingCollection
    {
        public static SettingCollection Current
        {
            get;
            private set;
        } = new SettingCollection();

        private SettingCollection()
            : base("Settings")
        {
            var clientSettings = Client.Current.Settings;
            clientSettings.PropertyChanged += this.ClientSettings_PropertyChanged;
        }

        private void ClientSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ExcludedTagNamespaces), nameof(ExcludedLanguages), nameof(ResampledImageSize),
                nameof(ExcludedUploaders));
        }

        private void update()
        {
            var task = Client.Current.Settings.SendAsync();
            task.Completed = (s, e) =>
            {
                if (e == Windows.Foundation.AsyncStatus.Error)
                {
                    Views.RootControl.RootController.SendToast(s.ErrorCode, null);
                }
            };
        }

        public void Apply()
        {
            this.VisitEx = this.VisitEx;
            this.OpenHVOnMonsterEncountered = this.OpenHVOnMonsterEncountered;
        }

        [Setting("Global", Index = 100)]
        [EnumRepresent("ApplicationTheme")]
        public ApplicationTheme Theme
        {
            get => GetLocal(ApplicationTheme.Dark);
            set
            {
                SetLocal(value);
                Window.Current.Content.Parent<FrameworkElement>().RequestedTheme = value.ToElementTheme();
                Themes.ThemeExtention.SetTitleBar();
            }
        }

        [Setting("Global", Index = 200)]
        public bool NeedVerify
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Global", Index = 300)]
        [ToggleSwitchRepresent("BooleanEx", "BooleanEh")]
        public bool VisitEx
        {
            get => GetRoaming(false);
            set
            {
                SetRoaming(value);
                Client.Current.Host = value ? HostType.Exhentai : HostType.Ehentai;
            }
        }

        [Setting("Global", Index = 400)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.YesNo)]
        public bool OpenHVOnMonsterEncountered
        {
            get => GetLocal(false);
            set
            {
                SetLocal(value);
                ExClient.HentaiVerse.HentaiVerseInfo.IsEnabled = value;
            }
        }

        [Setting("Global", Index = 500)]
        [ToggleSwitchRepresent("BooleanJT", "BooleanDT")]
        public bool UseJapaneseTitle
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 1100)]
        public string DefaultSearchString
        {
            get => GetRoaming("");
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 1200)]
        [CustomTemplate("CatagorySettingTemplate")]
        public Category DefaultSearchCategory
        {
            get => GetRoaming(Category.All);
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 1300)]
        public bool SaveLastSearch
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 1350)]
        [ToggleSwitchRepresent("BooleanByFavoritedTime", "BooleanByLastUpdatedTime")]
        public bool FavoritesOrderByFavoritedTime
        {
            get => Client.Current.Settings.FavoritesOrder == FavoritesOrder.ByFavoritedTime;
            set
            {
                if (value)
                    Client.Current.Settings.FavoritesOrder = FavoritesOrder.ByFavoritedTime;
                else
                    Client.Current.Settings.FavoritesOrder = FavoritesOrder.ByLastUpdatedTime;
                update();
            }
        }

        [Setting("Searching", Index = 1400)]
        [CustomTemplate("ExcludedTagNamespacesTemplate")]
        public Namespace ExcludedTagNamespaces
        {
            get => Client.Current.Settings.ExcludedTagNamespaces;
            set
            {
                Client.Current.Settings.ExcludedTagNamespaces = value;
                update();
            }
        }

        [Setting("Searching", Index = 1500)]
        [CustomTemplate("ExcludedLanguagesTemplate")]
        public string ExcludedLanguages
        {
            get => Client.Current.Settings.ExcludedLanguages.ToString();
            set
            {
                var el = Client.Current.Settings.ExcludedLanguages;
                el.Clear();
                el.AddRange(ExcludedLanguagesSettingProvider.FromString(value));
                update();
            }
        }

        [Setting("Searching", Index = 1600)]
        [TextTemplate(MultiLine = true)]
        public string ExcludedUploaders
        {
            get => Client.Current.Settings.ExcludedUploaders.ToString();
            set
            {
                var eu = Client.Current.Settings.ExcludedUploaders;
                eu.Clear();
                foreach (var item in ExcludedUploadersSettingProvider.FromString(value))
                {
                    eu.Add(item);
                }
                update();
            }
        }

        [Setting("Viewing", Index = 2100)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool UseChineseTagTranslation
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 2200)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool UseJapaneseTagTranslation
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 2250)]
        [EnumRepresent("CommentsOrderValues")]
        public CommentsOrder CommentsOrder
        {
            get => Client.Current.Settings.CommentsOrder;
            set
            {
                Client.Current.Settings.CommentsOrder = value;
                update();
            }
        }

        [Setting("Viewing", Index = 2300)]
        public string CommentTranslationCode
        {
            get => GetRoaming(Strings.Resources.General.LanguageCode);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 2400)]
        [ToggleSwitchRepresent("BooleanRightToLeft", "BooleanLeftToRight")]
        public bool ReverseFlowDirection
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Viewing", Index = 2500)]
        public bool KeepScreenOn
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Connection", Index = 9100)]
        [EnumRepresent("ImageSize")]
        public ImageSize ResampledImageSize
        {
            get => Client.Current.Settings.ResampledImageSize;
            set
            {
                Client.Current.Settings.ResampledImageSize = value;
                update();
            }
        }

        [Setting("Connection", Index = 9200)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.YesNo)]
        public bool LoadLofiOnMeteredInternetConnection
        {
            get => GetLocal(true);
            set
            {
                if (this.LoadLofiOnAllInternetConnection)
                    ForceSetLocal(true);
                else
                    ForceSetLocal(value);
            }
        }

        [Setting("Connection", Index = 9300)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.YesNo)]
        public bool LoadLofiOnAllInternetConnection
        {
            get => GetLocal(true);
            set
            {
                SetLocal(value);
                if (value)
                    this.LoadLofiOnMeteredInternetConnection = true;
            }
        }

        [Setting("About", Index = int.MaxValue)]
        [CustomTemplate("AboutContentTemplate")]
        public object AboutContent
        {
            get; set;
        }
    }
}
