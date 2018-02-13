using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Controls
{
    public class MyContentDialog : Windows.UI.Xaml.Controls.ContentDialog
    {
        public MyContentDialog()
        {
            // this.DefaultStyleKey = typeof(MyContentDialog);
            this.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            this.Loading += this.ContentDialog_Loading;
        }

        private void ContentDialog_Loading(FrameworkElement sender, object args)
        {
            var nextTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            if (sender.RequestedTheme != nextTheme)
                sender.RequestedTheme = nextTheme;
        }
    }
}
