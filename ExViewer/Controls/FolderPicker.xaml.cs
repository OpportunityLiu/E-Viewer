using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace ExViewer.Controls
{
    public sealed partial class FolderPicker : UserControl
    {
        public FolderPicker()
        {
            this.InitializeComponent();
        }

        public StorageFolder Folder
        {
            get => (StorageFolder)GetValue(FolderProperty);
            set => SetValue(FolderProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Folder"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register("Folder", typeof(StorageFolder), typeof(FolderPicker), new PropertyMetadata(null));

        public string FolderToken
        {
            get => (string)GetValue(FolderTokenProperty);
            set => SetValue(FolderTokenProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="FolderToken"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FolderTokenProperty =
            DependencyProperty.Register(nameof(FolderToken), typeof(string), typeof(FolderPicker), new PropertyMetadata("", FolderTokenPropertyChanged));

        private static async void FolderTokenPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (string)e.OldValue;
            var newValue = (string)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (FolderPicker)dp;
            if (!string.IsNullOrEmpty(oldValue))
                StorageApplicationPermissions.FutureAccessList.Remove(oldValue);
            if (string.IsNullOrEmpty(newValue))
                sender.Folder = null;
            else
                sender.Folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(newValue);
        }

        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var p = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder,
                SettingsIdentifier = "FP",
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
            };
            p.FileTypeFilter.Add(".");
            var f = await p.PickSingleFolderAsync();
            if (f is null)
                return;
            this.FolderToken = StorageApplicationPermissions.FutureAccessList.Add(f);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            this.FolderToken = "";
        }

        private async void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Folder != null)
                await Launcher.LaunchFolderAsync(this.Folder);
        }
    }
}
