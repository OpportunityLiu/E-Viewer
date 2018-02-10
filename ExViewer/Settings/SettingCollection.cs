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
            : base("Settings") { }

        public void Apply()
        {
            var clientSettings = Client.Current.Settings;
            this.ExcludedTagNamespaces = this.ExcludedTagNamespaces;
            this.ExcludedLanguages = this.ExcludedLanguages;
            this.VisitEx = this.VisitEx;
            this.OpenHVOnMonsterEncountered = this.OpenHVOnMonsterEncountered;
        }

        [Setting("Searching", Index = 10)]
        public string DefaultSearchString
        {
            get => GetRoaming("");
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 20)]
        [CustomTemplate("CatagorySettingTemplate")]
        public Category DefaultSearchCategory
        {
            get => GetRoaming(Category.All);
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 30)]
        public bool SaveLastSearch
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Searching", Index = 35)]
        [CustomTemplate("ExcludedTagNamespacesTemplate")]
        public Namespace ExcludedTagNamespaces
        {
            get => GetRoaming(Namespace.Unknown);
            set
            {
                SetRoaming(value);
                Client.Current.Settings.ExcludedTagNamespaces = value;
            }
        }

        [Setting("Searching", Index = 40)]
        [CustomTemplate("ExcludedLanguagesTemplate")]
        public string ExcludedLanguages
        {
            get => GetRoaming("");
            set
            {
                SetRoaming(value);
                var el = Client.Current.Settings.ExcludedLanguages;
                el.Clear();
                el.AddRange(ExcludedLanguagesSettingProvider.FromString(value));
            }
        }

        [Setting("Global", Index = -10)]
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

        [Setting("Global", Index = 10)]
        public bool NeedVerify
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Global", Index = 15)]
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

        [Setting("Global", Index = 17)]
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

        [Setting("Global", Index = 20)]
        [ToggleSwitchRepresent("BooleanJT", "BooleanDT")]
        public bool UseJapaneseTitle
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 20)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool UseChineseTagTranslation
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 30)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool UseJapaneseTagTranslation
        {
            get => GetRoaming(false);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 35)]
        public string CommentTranslationCode
        {
            get => GetRoaming(Strings.Resources.General.LanguageCode);
            set => SetRoaming(value);
        }

        [Setting("Viewing", Index = 40)]
        [ToggleSwitchRepresent("BooleanRightToLeft", "BooleanLeftToRight")]
        public bool ReverseFlowDirection
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Viewing", Index = 50)]
        public bool KeepScreenOn
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        [Setting("Connection", Index = 40)]
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

        [Setting("Connection", Index = 50)]
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
