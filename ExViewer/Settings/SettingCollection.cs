using ApplicationDataManager.Settings;
using System;
using ExClient;
using ExClient.Galleries;
using ExClient.Settings;
using ExClient.Tagging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Security.Credentials.UI;

namespace ExViewer.Settings
{
    public enum ViewOrientation
    {
        Auto,
        Horizontal,
        Vertical,
    }

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
            this.ImageCacheFolder = this.ImageCacheFolder;
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
                Themes.ThemeExtention.SetDefaltImage();
            }
        }

        [Setting("Global", Index = 200)]
        public bool NeedVerify
        {
            get => GetLocal(false);
            set
            {
                SetLocal(value);
                if (!value)
                    return;
                var test = UserConsentVerifier.CheckAvailabilityAsync();
                test.Completed = (s, e) =>
                {
                    var result = s.GetResults();
                    switch (result)
                    {
                    case UserConsentVerifierAvailability.DeviceNotPresent:
                        Views.RootControl.RootController.SendToast(Strings.Resources.Verify.DeviceNotPresent, null);
                        break;
                    case UserConsentVerifierAvailability.NotConfiguredForUser:
                        Views.RootControl.RootController.SendToast(Strings.Resources.Verify.NotConfigured, null);
                        break;
                    case UserConsentVerifierAvailability.DisabledByPolicy:
                        Views.RootControl.RootController.SendToast(Strings.Resources.Verify.Disabled, null);
                        break;
                    case UserConsentVerifierAvailability.DeviceBusy:
                        Views.RootControl.RootController.SendToast(Strings.Resources.Verify.DeviceBusy, null);
                        break;
                    default:
                        Views.RootControl.RootController.SendToast(Strings.Resources.Verify.OtherFailure, null);
                        break;
                    case UserConsentVerifierAvailability.Available:
                        return;
                    }
                    SetLocal(false, nameof(NeedVerify));
                };
            }
        }

        [Setting("Global", Index = 300)]
        [ToggleSwitchRepresent("BooleanEx", "BooleanEh")]
        public bool VisitEx
        {
            get => GetRoaming(false);
            set
            {
                SetRoaming(value);
                Client.Current.Settings.PropertyChanged -= this.ClientSettings_PropertyChanged;
                Client.Current.Host = value ? HostType.ExHentai : HostType.EHentai;
                this.ClientSettings_PropertyChanged(Client.Current.Settings, new System.ComponentModel.PropertyChangedEventArgs(null));
                Client.Current.Settings.PropertyChanged += this.ClientSettings_PropertyChanged;
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

        [Setting("Viewing", Index = 2350)]
        [EnumRepresent("ViewOrientation")]
        public ViewOrientation ImageViewOrientation
        {
            get => GetLocal(ViewOrientation.Auto);
            set => SetLocal(value);
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

        [Setting("Connection", Index = 9400)]
        [CustomTemplate("FolderTemplate")]
        public string ImageCacheFolder
        {
            get => GetLocal(default(string));
            set
            {
                SetLocal(value);
                setAsync(value);

                async void setAsync(string token)
                {
                    if (string.IsNullOrEmpty(token))
                        GalleryImage.ImageFolder = null;
                    else
                    {
                        try
                        {
                            GalleryImage.ImageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Custom cache folder set");
                        }
                        catch
                        {
                            ImageCacheFolder = null;
                        }
                    }
                }
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
