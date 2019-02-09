using ExClient.Api;
using ExClient.Tagging;
using ExViewer.ViewModels;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Linq;
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
        private class TagVM : ViewModelBase
        {
            public TagVM()
            {
                SubmitTag.Tag = this;
            }

            private GalleryTag selected;
            public GalleryTag SelectedTag { get => this.selected; set => Set(ref this.selected, value); }

            private TagCollection tags;
            public TagCollection Tags { get => this.tags; set => Set(ref this.tags, value); }

            public static bool CanVoteUp(TagState state) => state.GetVoteState() == TagState.NotPresented && !state.IsSlave();
            public static bool CanVoteDown(TagState state) => state.GetVoteState() == TagState.NotPresented;
            public static bool CanVoteWithdraw(TagState state) => state.GetVoteState() != TagState.NotPresented;

            public Visibility IsVoteUpVisible(TagState state) => CanVoteUp(state) ? Visibility.Visible : Visibility.Collapsed;
            public Visibility IsVoteDownVisible(TagState state) => CanVoteDown(state) ? Visibility.Visible : Visibility.Collapsed;
            public Visibility IsVoteWithdrawVisible(TagState state) => CanVoteWithdraw(state) ? Visibility.Visible : Visibility.Collapsed;

            public Command<string> CopyContent { get; } = Command<string>.Create((s, c) =>
            {
                var data = new DataPackage();
                data.SetText(c);
                Clipboard.SetContent(data);
                RootControl.RootController.SendToast(Strings.Resources.Controls.TagPresenter.TagCopied, null);
            }, (s, c) => !string.IsNullOrEmpty(c));

            public AsyncCommand<GalleryTag> VoteUp { get; }
                = AsyncCommand<GalleryTag>.Create((s, t) => t.VoteAsync(VoteState.Up), (s, t) => t != null);

            public AsyncCommand<GalleryTag> VoteDown { get; }
                = AsyncCommand<GalleryTag>.Create((s, t) => t.VoteAsync(VoteState.Down), (s, t) => t != null);

            public AsyncCommand<GalleryTag> VoteWithdraw { get; }
                = AsyncCommand<GalleryTag>.Create((s, t) => t.VoteAsync(VoteState.Default), (s, t) => t != null);

            private static EhWikiDialog ewd;
            public Command<GalleryTag> GoToDefination { get; } = Command<GalleryTag>.Create(async (s, t) =>
            {
                var dialog = System.Threading.LazyInitializer.EnsureInitialized(ref ewd);
                dialog.WikiTag = t.Content.Content;
                await dialog.ShowAsync();
            }, (s, t) => t?.Content.Content != null);

            public Command<GalleryTag> Search { get; } = Command<GalleryTag>.Create(async (s, t) =>
            {
                var vm = SearchVM.GetVM(t.Content.Search());
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery.ToString());
            }, (s, t) => t?.Content.Content != null);

            private static readonly char[] commas = ",՝،߸፣᠂⸲⸴⹁꘍꛵᠈꓾ʻʽ、﹐，﹑､︐︑".ToCharArray();
            public AsyncCommand<string> SubmitTag { get; } = AsyncCommand<string>.Create(async (sender, text) =>
            {
                var that = (TagVM)sender.Tag;
                var tagc = that.Tags;
                if (tagc is null)
                    return;

                var tags = text.Split(commas, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(ExClient.Tagging.Tag.Parse)
                    .Distinct();

                await tagc.VoteAsync(tags, VoteState.Up);
            });
        }

        public TagPresenter()
        {
            this.InitializeComponent();
            this.resetNewTagState();
            this.VM.SubmitTag.Executed += (s, e) =>
            {
                resetNewTagState();
            };
        }

        private readonly TagVM VM = new TagVM();

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
            sender.VM.Tags = (TagCollection)e.NewValue;
            sender.resetNewTagState();
        }

        private void gvTagGroup_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.VM.SelectedTag = (GalleryTag)e.ClickedItem;
            var s = (ListViewBase)sender;
            var container = (SelectorItem)s.ContainerFromItem(e.ClickedItem);
            this.mfoTag.ShowAt(container);
        }

        private void resetNewTagState()
        {
            this.btnStartNew.Visibility = Visibility.Visible;
            if (this.asbNewTags is null)
                return;
            this.asbNewTags.Visibility = Visibility.Collapsed;
            this.asbNewTags.Text = "";
            TagSuggestionService.IncreaseStateCode(this.asbNewTags);
        }

        private async void btnStartNew_Click(object sender, RoutedEventArgs e)
        {
            resetNewTagState();
            if (this.asbNewTags is null)
                FindName(nameof(this.asbNewTags));
            this.asbNewTags.Visibility = Visibility.Visible;
            await this.Dispatcher.YieldIdle();
            this.btnStartNew.Visibility = Visibility.Collapsed;
            this.asbNewTags.Focus(FocusState.Programmatic);
        }

        private void asbNewTags_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.asbNewTags.Text))
            {
                resetNewTagState();
            }
        }

        private void btnStartNew_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.btnStartNew.FocusState == FocusState.Keyboard)
            {
                btnStartNew_Click(sender, e);
            }
        }

        private async void tbContent_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var s = (TextBlock)sender;
            var newValue = (GalleryTag)args.NewValue;
            var oldValue = (GalleryTag)s.Tag;
            args.Handled = true;

            if (oldValue == newValue)
            {
                return;
            }

            s.Tag = newValue;
            var dc = newValue.Content.GetDisplayContentAsync();
            if (dc.Status == AsyncStatus.Completed)
            {
                s.Text = dc.GetResults();
                dc.Close();
            }
            else
            {
                s.Text = newValue.Content.Content;
                s.Text = await dc;
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
                return value.IsSlave() ? upSlaveBrush : upBrush;
            case TagState.Downvoted:
                return value.IsSlave() ? downSlaveBrush : downBrush;
            default:
                return value.IsSlave() ? slaveBrush : normalBrush;
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
