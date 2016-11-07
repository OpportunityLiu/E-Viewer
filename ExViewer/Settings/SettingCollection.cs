using ApplicationDataManager.Settings;
using ExClient;
using ExClient.Settings;
using Windows.UI.Xaml;

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

        [Setting("Searching", Index = 10)]
        public string DefaultSearchString
        {
            get
            {
                return GetRoaming("");
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting(
            "Searching",
            Index = 20,
            SettingPresenterTemplate = "CatagorySettingTemplate"
        )]
        public Category DefaultSearchCategory
        {
            get
            {
                return GetRoaming(Category.All);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Searching", Index = 30)]
        public bool SaveLastSearch
        {
            get
            {
                return GetRoaming(false);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Searching", Index = 40, SettingPresenterTemplate = "ExcludedLanguagesTemplate")]
        public string ExcludedLanguages
        {
            get
            {
                return GetRoaming("");
            }
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
            get
            {
                return GetLocal(ApplicationTheme.Dark);
            }
            set
            {
                SetLocal(value);
                ((FrameworkElement)Window.Current.Content).RequestedTheme = value.ToElementTheme();
                Themes.ThemeExtention.SetTitleBar();
            }
        }

        [Setting("Global", Index = 10)]
        public bool NeedVerify
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Global", Index = 20)]
        [BooleanRepresent("BooleanJT", "BooleanDT")]
        public bool UseJapaneseTitle
        {
            get
            {
                return GetRoaming(false);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Global", Index = 40)]
        [BooleanRepresent("BooleanEnabled", "BooleanDisabled")]
        public bool UseChineseTagTranslation
        {
            get
            {
                return GetRoaming(false);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Global", Index = 50)]
        [BooleanRepresent("BooleanEnabled", "BooleanDisabled")]
        public bool UseJapaneseTagTranslation
        {
            get
            {
                return GetRoaming(false);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("ImageViewing", Index = 10)]
        [SingleRange(1, 4, Small = 0.1)]
        public float DefaultFactor
        {
            get
            {
                return GetLocal(2f);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("ImageViewing", Index = 20)]
        [SingleRange(4, 10, Small = 0.1)]
        public float MaxFactor
        {
            get
            {
                return GetLocal(8f);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("ImageViewing", Index = 30)]
        [BooleanRepresent("BooleanEnabled", "BooleanDisabled")]
        public bool MouseInertial
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("ImageViewing", Index = 35)]
        [BooleanRepresent("BooleanRightToLeft", "BooleanLeftToRight")]
        public bool ReverseFlowDirection
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("ImageViewing", Index = 40)]
        [Int32Range(0, 1000, Tick = 100, Small = 10, Large = 100)]
        public int ChangeCommandBarDelay
        {
            get
            {
                return GetLocal(150);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("ImageViewing", Index = 45)]
        public bool KeepScreenOn
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Connection", Index = 10)]
        [BooleanRepresent("BooleanYes", "BooleanNo")]
        public bool LoadLofiOnMeteredInternetConnection
        {
            get
            {
                return GetLocal(true);
            }
            set
            {
                if(LoadLofiOnAllInternetConnection)
                    ForceSetLocal(true);
                else
                    ForceSetLocal(value);
            }
        }

        [Setting("Connection", Index = 20)]
        [BooleanRepresent("BooleanYes", "BooleanNo")]
        public bool LoadLofiOnAllInternetConnection
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
                if(value)
                    LoadLofiOnMeteredInternetConnection = true;
            }
        }

        [Setting("Hah", Index = 10)]
        public string HahAddress
        {
            get
            {
                return GetLocal("");
            }
            set
            {
                var old = GetLocal("");
                if(string.IsNullOrWhiteSpace(value))
                    value = "";
                try
                {
                    Client.Current.Settings.HahProxy.AddressAndPort = value;
                    ForceSetLocal(Client.Current.Settings.HahProxy.AddressAndPort);
                }
                catch(System.Exception ex)
                {
                    ForceSetLocal(old);
                    Views.RootControl.RootController.SendToast(ex, null);
                }
            }
        }

        [Setting("Hah", Index = 20)]
        public string HahPasskey
        {
            get
            {
                return GetLocal("");
            }
            set
            {
                Client.Current.Settings.HahProxy.Passkey = value;
                ForceSetLocal(Client.Current.Settings.HahProxy.Passkey);
            }
        }
    }
}
