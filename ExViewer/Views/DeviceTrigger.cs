using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Views
{
    class DeviceTrigger : StateTriggerBase
    {
        public static string DeviceType
        {
            get;
        } = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

        public static bool IsMobile
        {
            get;
        } = DeviceType == "Windows.Mobile";

        public static bool IsDesktop
        {
            get;
        } = DeviceType == "Windows.Desktop";

        public string ActiveType
        {
            get => (string)GetValue(ActiveTypeProperty);
            set => SetValue(ActiveTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ActiveType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveTypeProperty =
            DependencyProperty.Register("ActiveType", typeof(string), typeof(DeviceTrigger), new PropertyMetadata(null, PropertyChangedCallback));

        public static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DeviceTrigger)d).SetActive(e.NewValue.ToString() == DeviceType);
        }
    }
}

