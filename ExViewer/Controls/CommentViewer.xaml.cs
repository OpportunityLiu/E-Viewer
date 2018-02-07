using ExClient;
using ExClient.Galleries.Commenting;
using ExViewer.Views;
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
        public CommentViewer()
        {
            this.InitializeComponent();
        }

        private static EditCommentDialog editDialog = new EditCommentDialog();

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
            sender.TranslatedContent = null;
            if (e.NewValue is Comment c)
            {
                if (c.CanEdit)
                {
                    sender.FindName(nameof(AttentionHeader));
                }
            }
        }

        public HtmlAgilityPack.HtmlNode TranslatedContent
        {
            get => (HtmlAgilityPack.HtmlNode)GetValue(TranslatedContentProperty);
            set => SetValue(TranslatedContentProperty, value);
        }

        // Using a DependencyProperty as the backing store for TranslatedContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TranslatedContentProperty =
            DependencyProperty.Register("TranslatedContent", typeof(HtmlAgilityPack.HtmlNode), typeof(CommentViewer), new PropertyMetadata(null));

        protected override void OnDisconnectVisualChildren()
        {
            this.ClearValue(CommentProperty);
            base.OnDisconnectVisualChildren();
        }

        private void UserControl_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            FindName(nameof(this.EngagementIndicator));
        }

        private async void VoteUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.Comment.VoteAsync(ExClient.Api.VoteState.Up);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
        }

        private async void VoteDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await this.Comment.VoteAsync(ExClient.Api.VoteState.Down);
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, typeof(GalleryPage));
            }
        }

        private void VotePopUp_Opened(object sender, object e)
        {
            ((Popup)sender).Child.DescendantsAndSelf<Control>().First().Focus(FocusState.Programmatic);
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            editDialog.EditableComment = this.Comment;
            await editDialog.ShowAsync();
        }

        private void WithdrawVote_Click(object sender, RoutedEventArgs e)
        {
            switch (this.Comment.Status)
            {
            case CommentStatus.VotedUp:
                VoteUp_Click(sender, e);
                break;
            case CommentStatus.VotedDown:
                VoteDown_Click(sender, e);
                break;
            }
        }

        private async void Reply_Click(object sender, RoutedEventArgs e)
        {
            var dialog = System.Threading.LazyInitializer.EnsureInitialized(ref replyDialog);
            dialog.ReplyingComment = this.Comment;
            await dialog.ShowAsync();
        }

        private static ReplyCommentDialog replyDialog;

        private async void Translate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var translateTask = this.Comment.TranslateAsync(Settings.SettingCollection.Current.CommentTranslationCode);
                FindName(nameof(this.Translated));
                this.TranslatedContent = await translateTask;
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, null);
            }
        }

        private double toOpacity(HtmlAgilityPack.HtmlNode val)
        {
            if (val == null)
                return 1;
            return 0.7;
        }
    }
}
