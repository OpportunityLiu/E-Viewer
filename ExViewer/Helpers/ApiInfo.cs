using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Windows.Foundation.Metadata.ApiInformation;

namespace ExViewer
{
    public static class ApiInfo
    {
        [PlatformSpecific]
        public static bool CommandBarDynamicOverflowSupported
        {
            get;
        } = IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "IsDynamicOverflowEnabled");

        [PlatformSpecific]
        public static bool StatusBarSupported
        {
            get;
        } = IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        [PlatformSpecific]
        public static bool AnimatedGifSupported
        {
            get;
        } = IsPropertyPresent("Windows.UI.Xaml.Media.Imaging.BitmapImage", "AutoPlay");
    }
}
