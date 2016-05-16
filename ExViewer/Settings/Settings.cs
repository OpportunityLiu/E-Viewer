using Windows.UI.Xaml;

namespace ExViewer.Settings
{
    public partial class Settings : ExClient.ObservableObject
    {
        [Roaming]
        [Setting("Searching", "Default keywords on the front page", Index = 10)]
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

        [Roaming]
        [Setting(
            "Searching",
            "Default categories on the front page",
            Index = 20,
            SettingPresenterTemplate = "CatagorySettingTemplate"
        )]
        public ExClient.Category DefaultSearchCategory
        {
            get
            {
                return GetRoaming(ExClient.Category.All);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Overall", "The theme of the app (need restart the app)", Index = 10)]
        public ApplicationTheme Theme
        {
            get
            {
                return GetLocal(ApplicationTheme.Dark);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing", "Zoom factor for double tapping", Index = 10)]
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

        [Setting("Image viewing", "Maximum zoom factor", Index = 20)]
        [SingleRange(4, 8, Small = 0.1)]
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

        [Setting("Image viewing", "Factor for inertia of mouse dragging, set to 0 to disable", Index = 30)]
        [DoubleRange(0, 1, Small = 0.05)]
        public double MouseInertialFactor
        {
            get
            {
                return GetLocal(0.5);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing", "The latency for the command bar to hide or show after tapping", Index = 40)]
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
    }
}
