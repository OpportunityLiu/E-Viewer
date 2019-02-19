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
    public sealed partial class ExpungeGalleryDialog : MyContentDialog
    {
        private static readonly ExpungeReasonVM[] expungeReasonVM
            = EnumExtension.GetDefinedValues<ExpungeReason>().Select(kvp => new ExpungeReasonVM(kvp.Value)).ToArray();

        public ExpungeGalleryDialog()
        {
            InitializeComponent();
            PrimaryButtonText = Strings.Resources.General.Submit;
            CloseButtonText = Strings.Resources.General.Close;
            lvReason.ItemsSource = expungeReasonVM;
        }

        public Gallery Gallery
        {
            get => (Gallery)GetValue(GalleryProperty); set => SetValue(GalleryProperty, value);
        }

        private ExpungeInfo info;

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(AddToFavoritesDialog), new PropertyMetadata(null));

        private void resetVote()
        {
            lvReason.SelectedIndex = 0;
            tbExpl.Text = "";
        }

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
                    info = await Gallery.FetchExpungeInfoAsync();
                await info.VoteAsync(((ExpungeReasonVM)lvReason.SelectedItem).Key, tbExpl.Text);
                Bindings.Update();
                resetVote();
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
            if (Gallery.Expunged)
            {
                PrimaryButtonText = Strings.Resources.Views.ExpungeGalleryDialog.Expunged;
                IsPrimaryButtonEnabled = false;
            }
            try
            {
                pbLoading.IsIndeterminate = true;
                resetVote();
                await Dispatcher.YieldIdle();
                info = await Gallery.FetchExpungeInfoAsync();
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

        private void lvRecords_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (ExpungeRecord)e.ClickedItem;
            lvReason.SelectedItem = Array.Find(expungeReasonVM, i => i.Key == item.Reason);
            tbExpl.Text = item.Explanation;
        }

        private void MyContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            info = null;
            Bindings.Update();
            lvRecords.ItemsSource = null;
            tbInfo.Text = "";
            PrimaryButtonText = Strings.Resources.General.Submit;
            IsPrimaryButtonEnabled = true;
            resetVote();
        }

        private void lvReason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbInfo.Text = "";
        }

        private void tbExpl_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbInfo.Text = "";
        }


        static Visibility emptyVisible(int count, bool indeterminate)
        {
            if (count == 0 && !indeterminate)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }


        static Visibility loadingVisible(int count, bool indeterminate)
        {
            if (count == 0 && indeterminate)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }
    }

    public sealed class ExpungeReasonVM
    {
        public ExpungeReasonVM(ExpungeReason reason)
        {
            Name = reason.ToFriendlyNameString();
            Description = reason.GetDescription();
            Key = reason;
        }

        public string Name { get; }
        public string Description { get; }
        public ExpungeReason Key { get; }
    }
}
