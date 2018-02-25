using ExViewer.Controls;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class UpdateDialog : MyContentDialog
    {
        public UpdateDialog()
        {
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(VersionChecker.ReleaseUri);
        }

        internal VersionChecker.VersionInfo Version
        {
            get => (VersionChecker.VersionInfo)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(VersionChecker.VersionInfo), typeof(UpdateDialog), new PropertyMetadata(default(VersionChecker.VersionInfo), VersionPropertyChangedCallback));

        private static void VersionPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (UpdateDialog)d;
            var newValue = (VersionChecker.VersionInfo)e.NewValue;
            var v = newValue.Version;
            sender.tbTitle.Text = newValue.Title;
            sender.tbContent.Text = newValue.Content;
            sender.tbVersion.Text= string.Format(Strings.Resources.Views.UpdateDialog.ContentTemplate, v.Major, v.Minor, v.Build, v.Revision);
        }
    }
}
