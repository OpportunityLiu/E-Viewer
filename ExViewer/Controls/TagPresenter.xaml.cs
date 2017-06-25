using ExClient.Api;
using ExClient.Tagging;
using ExViewer.ViewModels;
using ExViewer.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class TagPresenter : UserControl
    {
        public TagPresenter()
        {
            this.InitializeComponent();
            this.submitCmd = new Opportunity.MvvmUniverse.Commands.AsyncCommand<string>(submit);
            this.resetNewTagState(false);
        }

        public TagCollection Tags
        {
            get => (TagCollection)GetValue(TagsProperty); set => SetValue(TagsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register(nameof(Tags), typeof(TagCollection), typeof(TagPresenter), new PropertyMetadata(null, TagsPropertyChanged));

        private static void TagsPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var sender = (TagPresenter)dp;
            sender.resetNewTagState(false);
        }

        private static readonly Brush upBrush = (Brush)Application.Current.Resources["VoteUpCommentBrush"];
        private static readonly Brush downBrush = (Brush)Application.Current.Resources["VoteDownCommentBrush"];
        private static readonly Brush upSlaveBrush = (Brush)Application.Current.Resources["SlaveVoteUpCommentBrush"];
        private static readonly Brush downSlaveBrush = (Brush)Application.Current.Resources["SlaveVoteDownCommentBrush"];
        private static readonly Brush slaveBrush = (Brush)Application.Current.Resources["SlaveCommentBrush"];

        private void tbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (TextBlock)sender;
            var value = (Tag)args.NewValue;
            args.Handled = true;
            var state = this.Tags.StateOf(value);

            switch (state.GetVoteState())
            {
            case TagState.Upvoted:
                if (state.IsSlave())
                    s.Foreground = upSlaveBrush;
                else
                    s.Foreground = upBrush;
                break;
            case TagState.Downvoted:
                if (state.IsSlave())
                    s.Foreground = downSlaveBrush;
                else
                    s.Foreground = downBrush;
                break;
            default:
                if (state.IsSlave())
                    s.Foreground = slaveBrush;
                else
                    s.ClearValue(TextBlock.ForegroundProperty);
                break;
            }

            switch (state.GetPowerState())
            {
            case TagState.LowPower:
                s.FontWeight = Windows.UI.Text.FontWeights.ExtraLight;
                break;
            case TagState.HighPower:
                s.FontWeight = Windows.UI.Text.FontWeights.Medium;
                break;
            }

            var dc = value.GetDisplayContentAsync();
            if (dc.Status == AsyncStatus.Completed)
            {
                s.Text = dc.GetResults();
            }
            else
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
                await this.Tags.VoteAsync(SelectedTag, VoteState.Up);
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
                await this.Tags.VoteAsync(SelectedTag, VoteState.Down);
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
                    await this.Tags.VoteAsync(SelectedTag, VoteState.Up);
                else if (state.HasFlag(TagState.Upvoted))
                    await this.Tags.VoteAsync(SelectedTag, VoteState.Down);
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
            RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery.ToString());
        }

        private static readonly char[] commas = new[] { ',', '՝', '،', '߸', '፣', '᠂', '⸲', '⸴', '⹁', '꘍', '꛵', '᠈', '꓾', 'ʻ', 'ʽ', '、', '﹐', '，', '﹑', '､', '︐', '︑' };

        private Opportunity.MvvmUniverse.Commands.AsyncCommand<string> submitCmd;

        private async Task submit(string text)
        {
            var tagc = this.Tags;
            if (tagc == null)
                return;
            try
            {
                this.asbNewTags.IsEnabled = false;
                var tags = text.Split(commas, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(ExClient.Tagging.Tag.Parse)
                    .Distinct().ToList();
                if (Tags.Count == 0)
                    return;
                await tagc.VoteAsync(tags, VoteState.Up);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
            finally
            {
                resetNewTagState(false);
            }
        }

        private void resetNewTagState(bool startToTag)
        {
            if (startToTag)
            {
                if (this.asbNewTags == null)
                    FindName(nameof(asbNewTags));
                this.asbNewTags.Visibility = Visibility.Visible;
                this.btnStartNew.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.asbNewTags != null)
                    this.asbNewTags.Visibility = Visibility.Collapsed;
                this.btnStartNew.Visibility = Visibility.Visible;
            }
            if (this.asbNewTags != null)
            {
                var id = TagSuggestionService.GetStateCode(this.asbNewTags);
                this.asbNewTags.Text = "";
                this.asbNewTags.IsEnabled = true;
                TagSuggestionService.SetStateCode(this.asbNewTags, id + 1);
            }
        }

        private async void btnStartNew_Click(object sender, RoutedEventArgs e)
        {
            var firstLoad = this.asbNewTags == null;
            resetNewTagState(true);
            if (firstLoad)
                return;
            await this.Dispatcher.Yield();
            this.asbNewTags.Focus(FocusState.Programmatic);
        }

        private async void asbNewTags_Loaded(object sender, RoutedEventArgs e)
        {
            await this.Dispatcher.Yield();
            this.asbNewTags.Focus(FocusState.Programmatic);
        }

        private void asbNewTags_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.asbNewTags.Text))
                resetNewTagState(false);
        }

        private void btnStartNew_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnStartNew.FocusState == FocusState.Keyboard)
                btnStartNew_Click(sender, e);
        }
    }
}
