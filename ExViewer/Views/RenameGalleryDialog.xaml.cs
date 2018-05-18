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
            this.InitializeComponent();
            this.PrimaryButtonText = Strings.Resources.General.Submit;
            this.CloseButtonText = Strings.Resources.General.Close;
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
            this.pbLoading.IsIndeterminate = true;
            this.tbInfo.Text = "";
            args.Cancel = true;
            var def = args.GetDeferral();
            try
            {
                await this.info.VoteAsync(this.tbRoman.Text, this.tbJapanese.Text);
                this.info.OnPropertyChanged(nameof(this.info.VotedRoman), nameof(this.info.VotedJapanese));
            }
            catch (Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                def.Complete();
                this.pbLoading.IsIndeterminate = false;
            }
        }

        private async void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            if (this.Gallery is null)
                throw new InvalidOperationException("Property RenameGalleryDialog.Gallery is null.");
            this.tbORoman.Text = this.Gallery.Title ?? "";
            this.tbOJapanese.Text = this.Gallery.TitleJpn ?? "";
            await Dispatcher.YieldIdle();
            try
            {
                this.pbLoading.IsIndeterminate = true;
                this.info = await this.Gallery.FetchRenameInfoAsync();
                this.Bindings.Update();
            }
            catch (Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                this.pbLoading.IsIndeterminate = false;
            }
        }

        private void MyContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.info = null;
            this.Bindings.Update();
            this.lvJapanese.ItemsSource = null;
            this.lvRoman.ItemsSource = null;
            this.tbInfo.Text = "";
            this.tbJapanese.Text = "";
            this.tbRoman.Text = "";
        }

        private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tbInfo.Text = "";
            var lv = (ListView)sender;
            var tb = lv.Descendants<TextBox>().First();
            if (lv.SelectedItem is RenameRecord r)
                tb.Text = r.Title;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbInfo.Text = "";
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
