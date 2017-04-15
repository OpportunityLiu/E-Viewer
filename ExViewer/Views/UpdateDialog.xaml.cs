using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using ExViewer.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using Windows.ApplicationModel;

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

        public PackageVersion Version
        {
            get => (PackageVersion)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(PackageVersion), typeof(UpdateDialog), new PropertyMetadata(default(PackageVersion), VersionPropertyChangedCallback));

        private static void VersionPropertyChangedCallback(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            var sender = (UpdateDialog)d;
            var newValue = (PackageVersion)e.NewValue;
            sender.Content = string.Format(Strings.Resources.Views.UpdateDialog.ContentTemplate, newValue.Major, newValue.Minor, newValue.Build, newValue.Revision);
        }
    }
}
