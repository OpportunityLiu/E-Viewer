using Opportunity.MvvmUniverse.Views;
using Windows.UI.Xaml;

namespace ExViewer.Controls
{
    public class MyContentDialog : MvvmContentDialog
    {
        public MyContentDialog()
        {
            DefaultStyleKey = typeof(MyContentDialog);
            RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            Loading += ContentDialog_Loading;
        }

        private void ContentDialog_Loading(FrameworkElement sender, object args)
        {
            var nextTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
            if (sender.RequestedTheme != nextTheme)
                sender.RequestedTheme = nextTheme;
        }
    }
}
