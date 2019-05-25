using ApplicationDataManager.Settings;
using ExClient;
using ExClient.Galleries;
using ExClient.Settings;
using ExClient.Tagging;
using System;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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
            clientSettings.PropertyChanged += ClientSettings_PropertyChanged;
        }

        private void ClientSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnObjectReset();
        }

        private async void _Update()
        {
            try
            {
                await Client.Current.Settings.SendAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Views.RootControl.RootController.SendToast(ex, null);
            }
        }

        public void Apply()
        {
            VisitEx = VisitEx;
            OpenHVOnMonsterEncountered = OpenHVOnMonsterEncountered;
            ImageCacheFolder = ImageCacheFolder;
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
                {
                    return;
                }

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
                Client.Current.Settings.PropertyChanged -= ClientSettings_PropertyChanged;
                Client.Current.Host = value ? HostType.ExHentai : HostType.EHentai;
                ClientSettings_PropertyChanged(Client.Current.Settings, new System.ComponentModel.PropertyChangedEventArgs(null));
                Client.Current.Settings.PropertyChanged += ClientSettings_PropertyChanged;
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

        [Setting("Global", Index = 800)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool TriggerDawnOfDay
        {
            get => GetLocal(false);
            set => SetLocal(value);
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
                {
                    Client.Current.Settings.FavoritesOrder = FavoritesOrder.ByFavoritedTime;
                }
                else
                {
                    Client.Current.Settings.FavoritesOrder = FavoritesOrder.ByLastUpdatedTime;
                }

                _Update();
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
                _Update();
            }
        }

        [Setting("Searching", Index = 1450)]
        [TextTemplate]
        public int TagFilteringThreshold
        {
            get => Client.Current.Settings.TagFilteringThreshold;
            set
            {
                Client.Current.Settings.TagFilteringThreshold = value;
                _Update();
            }
        }

        [Setting("Searching", Index = 1470)]
        [TextTemplate]
        public int TagWatchingThreshold
        {
            get => Client.Current.Settings.TagWatchingThreshold;
            set
            {
                Client.Current.Settings.TagWatchingThreshold = value;
                _Update();
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
                _Update();
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
                _Update();
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
                _Update();
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
            get => GetLocal(ViewOrientation.Horizontal);
            set => SetLocal(value);
        }

        [Setting("Viewing", Index = 2400)]
        [ToggleSwitchRepresent("BooleanRightToLeft", "BooleanLeftToRight")]
        public bool ReverseFlowDirection
        {
            get => GetLocal(false);
            set => SetLocal(value);
        }

        /// <summary>
        /// Tap in image view to previous/next page.
        /// </summary>
        [Setting("Viewing", Index = 2450)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.EnabledDisabled)]
        public bool TapToFlip
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

        [Setting("Viewing", Index = 2500)]
        [Range(1, 16, ApplicationDataManager.Settings.ValueType.Double, Large = 5, Small = 0.1, Tick = 1)]
        public double SlideInterval
        {
            get => GetLocal(5.0);
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
                _Update();
            }
        }

        [Setting("Connection", Index = 9200)]
        [ToggleSwitchRepresent(PredefinedToggleSwitchRepresent.YesNo)]
        public bool LoadLofiOnMeteredInternetConnection
        {
            get => GetLocal(true);
            set
            {
                if (LoadLofiOnAllInternetConnection)
                {
                    ForceSetLocal(true);
                }
                else
                {
                    ForceSetLocal(value);
                }
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
                {
                    LoadLofiOnMeteredInternetConnection = true;
                }
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
                    {
                        await Config.SetImageFolderAsync(null);
                    }
                    else
                    {
                        try
                        {
                            await Config.SetImageFolderAsync(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token));
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
