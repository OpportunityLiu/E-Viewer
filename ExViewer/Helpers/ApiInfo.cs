using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Windows.Foundation.Metadata.ApiInformation;

namespace ExViewer
{
    public static class ApiInfo
    {
        public static bool CommandBarDynamicOverflowSupported
        {
            get;
        } = IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "IsDynamicOverflowEnabled");

        public static bool StatusBarSupported
        {
            get;
        } = IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        public static bool AnimatedGifSupported
        {
            get;
        } = IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay");
    }
}
