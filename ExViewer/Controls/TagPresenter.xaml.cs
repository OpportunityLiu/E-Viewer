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
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class TagPresenter : UserControl
    {
        public TagPresenter()
        {
            this.InitializeComponent();
            this.submitCmd.Tag = this;
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

        private void gvTagGroup_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.SelectedTag = (GalleryTag)e.ClickedItem;
            updateState();
            var s = (ListViewBase)sender;
            var container = (SelectorItem)s.ContainerFromItem(e.ClickedItem);
            this.mfoTag.ShowAt(container);
        }

        private void updateState()
        {
            if (Tags == null || SelectedTag.Content == null)
                return;

            var state = SelectedTag.State;
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

        public GalleryTag SelectedTag
        {
            get => (GalleryTag)GetValue(SelectedTagProperty); set => SetValue(SelectedTagProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTagProperty =
            DependencyProperty.Register("SelectedTag", typeof(GalleryTag), typeof(TagPresenter), new PropertyMetadata(default(GalleryTag)));

        private void mfiContent_Click(object sender, RoutedEventArgs e)
        {
            var content = SelectedTag.Content.Content;
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
                await SelectedTag?.VoteAsync(VoteState.Up);
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
                await SelectedTag?.VoteAsync(VoteState.Down);
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
                await SelectedTag?.VoteAsync(VoteState.Default);
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
            dialog.WikiTag = SelectedTag.Content;
            await dialog.ShowAsync();
        }

        private void mfiTagSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTag.Content == null)
                return;
            var vm = SearchVM.GetVM(SelectedTag.Content.Search());
            RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery.ToString());
        }

        private static readonly char[] commas = new[] { ',', '՝', '،', '߸', '፣', '᠂', '⸲', '⸴', '⹁', '꘍', '꛵', '᠈', '꓾', 'ʻ', 'ʽ', '、', '﹐', '，', '﹑', '､', '︐', '︑' };

        private Opportunity.MvvmUniverse.Commands.AsyncCommand<string> submitCmd =
            Opportunity.MvvmUniverse.Commands.AsyncCommand.Create<string>(async (sender, text) =>
        {
            var that = (TagPresenter)sender.Tag;
            var tagc = that.Tags;
            if (tagc == null)
                return;
            try
            {
                that.asbNewTags.IsEnabled = false;
                var tags = text.Split(commas, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(ExClient.Tagging.Tag.Parse)
                    .Distinct().ToList();
                if (that.Tags.Count == 0)
                    return;
                await tagc.VoteAsync(tags, VoteState.Up);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
            finally
            {
                that.resetNewTagState(false);
            }
        });

        private void resetNewTagState(bool startToTag)
        {
            if (startToTag)
            {
                if (this.asbNewTags == null)
                    FindName(nameof(this.asbNewTags));
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

        private void btnStartNew_Click(object sender, RoutedEventArgs e)
        {
            var firstLoad = this.asbNewTags == null;
            resetNewTagState(true);
            if (!firstLoad)
                asbNewTags_Loaded(this.asbNewTags, e);
        }

        private async void asbNewTags_Loaded(object sender, RoutedEventArgs e)
        {
            await this.Dispatcher.YieldIdle();
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

        private void tbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (TextBlock)sender;
            var newValue = (GalleryTag)args.NewValue;
            var oldValue = (GalleryTag)s.Tag;
            args.Handled = true;

            if (oldValue == newValue)
                return;
            s.Tag = newValue;
            var dc = newValue.Content.GetDisplayContentAsync();
            if (dc.Status == AsyncStatus.Completed)
            {
                s.Text = dc.GetResults();
            }
            else
            {
                s.Text = newValue.Content.Content;
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

        private static Windows.UI.Text.FontWeight TagStateToFontWeight(TagState value)
        {
            switch (value.GetPowerState())
            {
            case TagState.LowPower:
                return Windows.UI.Text.FontWeights.ExtraLight;
            case TagState.HighPower:
                return Windows.UI.Text.FontWeights.Medium;
            default:
                return Windows.UI.Text.FontWeights.Normal;
            }
        }

        private static Brush TagStateToBrush(TagState value)
        {
            switch (value.GetVoteState())
            {
            case TagState.Upvoted:
                if (value.IsSlave())
                    return upSlaveBrush;
                else
                    return upBrush;
            case TagState.Downvoted:
                if (value.IsSlave())
                    return downSlaveBrush;
                else
                    return downBrush;
            default:
                if (value.IsSlave())
                    return slaveBrush;
                else
                    return normalBrush;
            }
        }

        private static readonly Brush normalBrush = (Brush)Application.Current.Resources["NormalTagBrush"];
        private static readonly Brush slaveBrush = (Brush)Application.Current.Resources["SlaveTagBrush"];

        private static readonly Brush upBrush = (Brush)Application.Current.Resources["VoteUpTagBrush"];
        private static readonly Brush upSlaveBrush = (Brush)Application.Current.Resources["SlaveVoteUpTagBrush"];

        private static readonly Brush downBrush = (Brush)Application.Current.Resources["VoteDownTagBrush"];
        private static readonly Brush downSlaveBrush = (Brush)Application.Current.Resources["SlaveVoteDownTagBrush"];
    }
}
