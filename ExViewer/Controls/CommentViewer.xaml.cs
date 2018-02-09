using ExClient;
using ExClient.Galleries.Commenting;
using ExViewer.Views;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class CommentViewer : UserControl
    {
        private class CommentVM : ViewModelBase
        {
            public CommentVM()
            {
                this.Translate.Tag = this;
            }

            private Comment comment;
            public Comment Comment
            {
                get => this.comment;
                set
                {
                    if (Set(ref this.comment, value))
                    {
                        TranslatedContent = null;
                    }
                }
            }

            public static bool CanVoteUp(CommentStatus status) => status == CommentStatus.Votable || status == CommentStatus.VotedDown;
            public static bool CanVoteDown(CommentStatus status) => status == CommentStatus.Votable || status == CommentStatus.VotedUp;
            public static bool CanVoteWithdraw(CommentStatus status) => status == CommentStatus.VotedUp || status == CommentStatus.VotedDown;

            public Visibility IsVoteUpVisible(CommentStatus status) => CanVoteUp(status) ? Visibility.Visible : Visibility.Collapsed;
            public Visibility IsVoteDownVisible(CommentStatus status) => CanVoteDown(status) ? Visibility.Visible : Visibility.Collapsed;
            public Visibility IsVoteWithdrawVisible(CommentStatus status) => CanVoteWithdraw(status) ? Visibility.Visible : Visibility.Collapsed;

            private HtmlAgilityPack.HtmlNode translated;
            public HtmlAgilityPack.HtmlNode TranslatedContent
            {
                get => this.translated;
                private set
                {
                    if (Set(ref this.translated, value))
                        Translate.OnCanExecuteChanged();
                }
            }

            public AsyncCommand<Comment> Translate { get; } = AsyncCommand.Create<Comment>(async (s, c) =>
            {
                var r = await c.TranslateAsync(Settings.SettingCollection.Current.CommentTranslationCode);
                ((CommentVM)s.Tag).TranslatedContent = r;
            }, (s, c) => c != null && ((CommentVM)s.Tag).translated == null);

            public AsyncCommand<Comment> VoteUp { get; }
                = AsyncCommand.Create<Comment>((s, c) => c.VoteAsync(ExClient.Api.VoteState.Up), (s, c) => c != null && CanVoteUp(c.Status));

            public AsyncCommand<Comment> VoteDown { get; }
                = AsyncCommand.Create<Comment>((s, c) => c.VoteAsync(ExClient.Api.VoteState.Down), (s, c) => c != null && CanVoteDown(c.Status));

            public AsyncCommand<Comment> VoteWithdraw { get; }
                = AsyncCommand.Create<Comment>((s, c) => c.VoteAsync(ExClient.Api.VoteState.Default), (s, c) => c != null && CanVoteWithdraw(c.Status));

            private static EditCommentDialog editDialog;
            public Command<Comment> Edit { get; } = Command.Create<Comment>(async (s, c) =>
            {
                var dialog = System.Threading.LazyInitializer.EnsureInitialized(ref editDialog);
                dialog.EditableComment = c;
                await dialog.ShowAsync();
            }, (s, c) => c != null && c.CanEdit);

            private static ReplyCommentDialog replyDialog;
            public Command<Comment> Reply { get; } = Command.Create<Comment>(async (s, c) =>
            {
                var dialog = System.Threading.LazyInitializer.EnsureInitialized(ref replyDialog);
                dialog.ReplyingComment = c;
                await dialog.ShowAsync();
            }, (s, c) => c != null && !c.CanEdit);
        }

        public CommentViewer()
        {
            this.InitializeComponent();
            this.VM.Translate.Executed += (s, e) =>
            {
                var ex = e.Exception;
                if (ex != null)
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                else
                    FindName(nameof(this.Translated));
            };
            this.VM.PropertyChanged += this.VM_PropertyChanged;
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = (CommentVM)sender;
            if (vm.Comment?.CanEdit ?? false)
                FindName(nameof(this.AttentionHeader));
        }

        private readonly CommentVM VM = new CommentVM();

        public Comment Comment
        {
            get => (Comment)GetValue(CommentProperty);
            set => SetValue(CommentProperty, value);
        }

        // Using a DependencyProperty as the backing store for Comment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(Comment), typeof(CommentViewer), new PropertyMetadata(null, CommentPropertyChanged));

        private static void CommentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CommentViewer)d;
            sender.VM.Comment = (Comment)e.NewValue;
        }

        protected override void OnDisconnectVisualChildren()
        {
            this.ClearValue(CommentProperty);
            base.OnDisconnectVisualChildren();
        }

        private void UserControl_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            FindName(nameof(this.EngagementIndicator));
        }

        private static double toOpacity(HtmlAgilityPack.HtmlNode val)
        {
            if (val == null)
                return 1;
            return 0.7;
        }
    }
}
