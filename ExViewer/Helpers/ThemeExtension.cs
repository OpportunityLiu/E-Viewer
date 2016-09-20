using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer
{
    internal static class ThemeExtension
    {
        public static ElementTheme ToElementTheme(this ApplicationTheme value)
        {
            switch(value)
            {
            case ApplicationTheme.Light:
                return ElementTheme.Light;
            case ApplicationTheme.Dark:
                return ElementTheme.Dark;
            default:
                return ElementTheme.Default;
            }
        }

        public static ApplicationTheme ToApplicationTheme(this ElementTheme value)
        {
            switch(value)
            {
            case ElementTheme.Light:
                return ApplicationTheme.Light;
            case ElementTheme.Dark:
                return ApplicationTheme.Dark;
            default:
                return Application.Current.RequestedTheme;
            }
        }
    }
}
