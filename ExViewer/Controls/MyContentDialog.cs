using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Controls
{
    public class MyContentDialog : Windows.UI.Xaml.Controls.ContentDialog
    {
        public MyContentDialog()
        {
            this.Loading += this.ContentDialog_Loading;
        }

        private void ContentDialog_Loading(Windows.UI.Xaml.FrameworkElement sender, object args)
        {
            sender.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
        }
    }
}
