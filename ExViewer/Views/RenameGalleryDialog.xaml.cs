using ExClient;
using ExClient.Galleries;
using ExClient.Services;
using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class RenameGalleryDialog : MyContentDialog
    {
        public RenameGalleryDialog()
        {
            InitializeComponent();
            PrimaryButtonText = Strings.Resources.General.Submit;
            CloseButtonText = Strings.Resources.General.Close;
        }

        public Gallery Gallery
        {
            get => (Gallery)GetValue(GalleryProperty); set => SetValue(GalleryProperty, value);
        }

        private RenameInfo info;

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(AddToFavoritesDialog), new PropertyMetadata(null));

        private async void MyContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var def = args.GetDeferral();
            args.Cancel = true;
            try
            {
                pbLoading.IsIndeterminate = true;
                tbInfo.Text = "";
                await Dispatcher.YieldIdle();
                if (info is null)
                    info = await Gallery.FetchRenameInfoAsync();
                await info.VoteAsync(tbRoman.Text, tbJapanese.Text);
                Bindings.Update();
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                def.Complete();
                pbLoading.IsIndeterminate = false;
            }
        }

        private async void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            if (Gallery is null)
                throw new InvalidOperationException("Property RenameGalleryDialog.Gallery is null.");
            try
            {
                pbLoading.IsIndeterminate = true;
                await Dispatcher.YieldIdle();
                lvRoman.Header = Gallery.Title ?? "";
                lvJapanese.Header = Gallery.TitleJpn ?? "";
                info = await Gallery.FetchRenameInfoAsync();
                Bindings.Update();
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                pbLoading.IsIndeterminate = false;
            }
        }

        private async void MyContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // resize the dialog.
            var d = args.GetDeferral();
            await Task.Delay(33);
            d.Complete();
        }

        private void MyContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            info = null;
            Bindings.Update();
            lvJapanese.ItemsSource = null;
            lvRoman.ItemsSource = null;
            tbInfo.Text = "";
            tbJapanese.Text = "";
            tbRoman.Text = "";
        }

        private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbInfo.Text = "";
            var lv = (ListView)sender;
            var tb = lv.Descendants<TextBox>().First();
            if (lv.SelectedItem is RenameRecord r)
                tb.Text = r.Title;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbInfo.Text = "";
            var tb = (TextBox)sender;
            var focused = tb.FocusState;
            var lv = tb.Ancestors<ListView>().First();
            var source = (IList<RenameRecord>)lv.ItemsSource;
            var item = source.FirstOrDefault(r => r.Title == tb.Text);
            lv.SelectedItem = item;
            if (focused != FocusState.Unfocused)
                tb.Focus(focused);
        }
    }
}
