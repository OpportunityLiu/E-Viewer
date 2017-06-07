using ExClient;
using ExClient.Api;
using ExClient.Galleries;
using ExClient.Tagging;
using ExViewer.ViewModels;
using ExViewer.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class TagPresenter : UserControl
    {
        public TagPresenter()
        {
            this.InitializeComponent();
        }

        public TagCollection Tags
        {
            get => (TagCollection)GetValue(TagsProperty); set => SetValue(TagsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register(nameof(Tags), typeof(TagCollection), typeof(TagPresenter), new PropertyMetadata(null));

        private void tbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (TextBlock)sender;
            var value = (Tag)args.NewValue;
            args.Handled = true;
            if (value.Content == null)
            {
                s.ClearValue(TextBlock.TextProperty);
                return;
            }
            var state = this.Tags.StateOf(value);
            if(state.HasFlag(TagState.Slave))
            {
                s.Opacity = 0.4;
            }
            else
            {
                s.ClearValue(OpacityProperty);
            }
            var dc = value.GetDisplayContentAsync();
            if (dc.Status != AsyncStatus.Completed)
            {
                s.Text = value.Content;
                dc.Completed = (IAsyncOperation<string> op, AsyncStatus e) =>
                {
                    if (e != AsyncStatus.Completed)
                        return;
                    var dispValue = op.GetResults();
                    Opportunity.MvvmUniverse.DispatcherHelper.BeginInvokeOnUIThread(() =>
                    {
                        s.Text = dispValue;
                    });
                };
            }
            else
            {
                s.Text = dc.GetResults();
            }
        }

        private void gvTagGroup_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.SelectedTag = (Tag)e.ClickedItem;
            updateState();
            var s = (ListViewBase)sender;
            var container = (SelectorItem)s.ContainerFromItem(e.ClickedItem);
            this.mfoTag.ShowAt(container);
        }

        private void updateState()
        {
            if (Tags == null || SelectedTag.Content == null)
                return;

            var state = Tags.StateOf(SelectedTag);
            var dv = state.HasFlag(TagState.Downvoted);
            var uv = state.HasFlag(TagState.Upvoted);
            var sl = state.HasFlag(TagState.Slave);
            var canVU = !dv && !uv && !sl;
            var canVD = !dv && !uv;
            var canWV = dv || uv;
            this.mfiUp.Visibility = canVU ? Visibility.Visible : Visibility.Collapsed;
            this.mfiDown.Visibility = canVD ? Visibility.Visible : Visibility.Collapsed;
            this.mfiWithdraw.Visibility = canWV ? Visibility.Visible : Visibility.Collapsed;
        }

        public Tag SelectedTag
        {
            get => (Tag)GetValue(SelectedTagProperty); set => SetValue(SelectedTagProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTagProperty =
            DependencyProperty.Register("SelectedTag", typeof(Tag), typeof(TagPresenter), new PropertyMetadata(default(Tag)));

        private void mfiContent_Click(object sender, RoutedEventArgs e)
        {
            var content = SelectedTag.Content;
            if (content == null)
                return;
            var data = new DataPackage();
            data.SetText(content);
            Clipboard.SetContent(data);
            RootControl.RootController.SendToast(Strings.Resources.Controls.TagPresenter.TagCopied, typeof(GalleryPage));
        }

        private async void mfiUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.Tags.VoteAsync(SelectedTag, VoteCommand.Up);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
            this.updateState();
        }

        private async void mfiDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.Tags.VoteAsync(SelectedTag, VoteCommand.Down);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
            this.updateState();
        }

        private async void mfiWithdraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var state = this.Tags.StateOf(SelectedTag);
                if (state.HasFlag(TagState.Downvoted))
                    await this.Tags.VoteAsync(SelectedTag, VoteCommand.Up);
                else if (state.HasFlag(TagState.Upvoted))
                    await this.Tags.VoteAsync(SelectedTag, VoteCommand.Down);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
            this.updateState();
        }

        private static EhWikiDialog ewd;

        private async void mfiTagDefination_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTag.Content == null)
                return;
            var dialog = System.Threading.LazyInitializer.EnsureInitialized(ref ewd);
            dialog.WikiTag = SelectedTag;
            await dialog.ShowAsync();
        }

        private void mfiTagSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTag.Content == null)
                return;
            var vm = SearchVM.GetVM(SelectedTag.Search());
            RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery);
        }
    }
}
