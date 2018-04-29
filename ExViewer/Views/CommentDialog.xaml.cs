using ExClient.Galleries;
using ExClient.Galleries.Commenting;
using ExViewer.Controls;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public partial class CommentDialog : MyContentDialog
    {
        public CommentDialog()
        {
            this.InitializeComponent();
        }

        private void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            this.tbInfo.Text = "";
        }

        private void tbContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbInfo.Text = "";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }

        private async void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // resize the dialog.
            var d = args.GetDeferral();
            await Task.Delay(33);
            d.Complete();
        }

        private void abb_Click(object sender, RoutedEventArgs e)
        {
            var tag = ((FrameworkElement)sender).Tag.ToString();
            handleTag(tag);
        }

        private string safeSubstring(string str, int start, int length)
        {
            if (start < 0)
            {
                length += start;
                start = 0;
            }
            if (length < 0)
            {
                return "";
            }

            if (start + length > str.Length)
            {
                return str.Substring(start);
            }

            return str.Substring(start, length);
        }

        private void handleTag(string tag)
        {
            if (this.tbContent.IsReadOnly)
            {
                return;
            }

            if (tag == "url")
            {
                handleUrl();
                return;
            }
            var begin = $"[{tag}]";
            var end = $"[/{tag}]";
            var text = this.tbContent.Text;
            var sS = this.tbContent.SelectionStart;
            var currentSelected = this.tbContent.SelectedText;
            bool? startsWith = false;
            bool? endsWith = false;
            if (currentSelected.StartsWith(begin))
            {
                startsWith = true;
            }
            else if (safeSubstring(text, sS - begin.Length, begin.Length) == begin)
            {
                startsWith = null;
                sS -= begin.Length;
                this.tbContent.Select(sS, currentSelected.Length + begin.Length);
                currentSelected = this.tbContent.SelectedText;
            }
            if (currentSelected.EndsWith(end))
            {
                endsWith = true;
            }
            else if (safeSubstring(text, sS + currentSelected.Length, end.Length) == end)
            {
                endsWith = null;
                this.tbContent.Select(sS, currentSelected.Length + end.Length);
                currentSelected = this.tbContent.SelectedText;
            }
            if (startsWith is null && endsWith == false)
            {
                startsWith = false;
                sS += begin.Length;
                this.tbContent.Select(sS, currentSelected.Length - begin.Length);
                currentSelected = this.tbContent.SelectedText;
            }
            else if (startsWith == false && endsWith is null)
            {
                endsWith = false;
                this.tbContent.Select(sS, currentSelected.Length - end.Length);
                currentSelected = this.tbContent.SelectedText;
            }
            if (startsWith != false && endsWith != false)
            {
                this.tbContent.SelectedText = currentSelected.Substring(begin.Length, currentSelected.Length - begin.Length - end.Length);
                return;
            }
            var replaced = string.Concat(begin, currentSelected, end);
            this.tbContent.SelectedText = replaced;
            var s = this.tbContent.SelectionStart;
            if (currentSelected.Length == 0)
            {
                this.tbContent.Select(s + replaced.Length - end.Length, 0);
            }
        }

        private static Regex urlRegex = new Regex(@"^\[url(?:=[^\]]*?)?\](.*)\[/url\]$", RegexOptions.Compiled);

        private void handleUrl()
        {
            var currentSelected = this.tbContent.SelectedText;
            var end = "[/url]";
            var match = urlRegex.Match(currentSelected);
            if (match.Success)
            {
                this.tbContent.SelectedText = match.Groups[1].Value;
                return;
            }
            if (Uri.TryCreate(currentSelected, UriKind.Absolute, out var uri) &&
                (uri.Host.EndsWith("e-hentai.org") || uri.Host.EndsWith("exhentai.org")))
            {
                var begin = $"[url={currentSelected}]";
                var replaced = string.Concat(begin, currentSelected, end);
                this.tbContent.SelectedText = replaced;
                var s = this.tbContent.SelectionStart;
                this.tbContent.Select(s + begin.Length, currentSelected.Length);
            }
            else
            {
                const string begin = "[url]";
                var replaced = string.Concat(begin, currentSelected, end);
                this.tbContent.SelectedText = replaced;
                var s = this.tbContent.SelectionStart;
                if (currentSelected.Length == 0)
                {
                    this.tbContent.Select(s + replaced.Length - end.Length, 0);
                }
            }
        }

        private bool ctrlDown;

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Control ||
                e.OriginalKey == Windows.System.VirtualKey.LeftControl ||
                e.OriginalKey == Windows.System.VirtualKey.RightControl)
            {
                this.ctrlDown = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Control ||
                e.OriginalKey == Windows.System.VirtualKey.LeftControl ||
                e.OriginalKey == Windows.System.VirtualKey.RightControl)
            {
                this.ctrlDown = false;
            }
            base.OnKeyUp(e);
            if (e.Handled || !this.ctrlDown)
            {
                return;
            }

            e.Handled = true;
            switch (e.OriginalKey)
            {
            case Windows.System.VirtualKey.B:
                handleTag("b");
                break;
            case Windows.System.VirtualKey.T:
                handleTag("i");
                break;
            case Windows.System.VirtualKey.U:
                handleTag("u");
                break;
            case Windows.System.VirtualKey.S:
                handleTag("s");
                break;
            case Windows.System.VirtualKey.L:
                handleTag("url");
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 320)
            {
                this.gdEditBar.HorizontalAlignment = HorizontalAlignment.Right;
                this.gdEditBar.Width = 320;
            }
            else
            {
                this.gdEditBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.gdEditBar.Width = double.NaN;
            }
        }
    }

    public sealed class AddCommentDialog : CommentDialog
    {
        public AddCommentDialog()
        {
            this.Title = Strings.Resources.Views.CommentDialog.AddTitle;
            this.PrimaryButtonClick += this.AddCommentDialog_PrimaryButtonClick;
            this.Opened += this.AddCommentDialog_Opened;
        }

        public Gallery Gallery
        {
            get => (Gallery)GetValue(GalleryProperty);
            set => SetValue(GalleryProperty, value);
        }

        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register(nameof(Gallery), typeof(Gallery), typeof(AddCommentDialog), new PropertyMetadata(null));

        private void AddCommentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (Gallery is null)
            {
                throw new InvalidOperationException();
            }

            this.tbContent.Text = "";
            this.tbContent.Focus(FocusState.Programmatic);
        }

        private async void AddCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await Gallery.Comments.PostCommentAsync(this.tbContent.Text);
            }
            catch (Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                this.pbLoading.IsIndeterminate = false;
            }
        }
    }

    public sealed class EditCommentDialog : CommentDialog
    {
        public EditCommentDialog()
        {
            this.Title = Strings.Resources.Views.CommentDialog.EditTitle;
            this.Opened += this.EditCommentDialog_Opened;
            this.PrimaryButtonClick += this.EditCommentDialog_PrimaryButtonClick;
        }

        public Comment EditableComment
        {
            get => (Comment)GetValue(EditableCommentProperty);
            set => SetValue(EditableCommentProperty, value);
        }

        public static readonly DependencyProperty EditableCommentProperty =
            DependencyProperty.Register(nameof(EditableComment), typeof(Comment), typeof(EditCommentDialog), new PropertyMetadata(null, EditableCommentPropertyChangedCallback));

        private static void EditableCommentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (EditCommentDialog)d;
            var newValue = (Comment)e.NewValue;
            if (newValue != null && !newValue.CanEdit)
            {
                throw new InvalidOperationException();
            }
        }

        private async void EditCommentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.tbContent.Text = "";
            if (EditableComment is null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                this.pbLoading.IsIndeterminate = true;
                var editable = await EditableComment.FetchEditableAsync() ?? "";
                this.tbContent.Text = editable;
                this.tbContent.Select(editable.Length, 0);
            }
            catch (Exception ex)
            {
                this.tbContent.Text = "";
                this.tbInfo.Text = ex.GetMessage();
            }
            finally
            {
                this.pbLoading.IsIndeterminate = false;
            }
            this.tbContent.Focus(FocusState.Programmatic);
        }

        private async void EditCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await EditableComment.EditAsync(this.tbContent.Text);
            }
            catch (Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                this.pbLoading.IsIndeterminate = false;
            }
        }
    }

    public sealed class ReplyCommentDialog : CommentDialog
    {
        public ReplyCommentDialog()
        {
            this.Title = Strings.Resources.Views.CommentDialog.AddTitle;
            this.Opened += this.ReplyCommentDialog_Opened;
            this.PrimaryButtonClick += this.EditCommentDialog_PrimaryButtonClick;
        }

        public Comment ReplyingComment
        {
            get => (Comment)GetValue(ReplyingCommentProperty);
            set => SetValue(ReplyingCommentProperty, value);
        }

        public static readonly DependencyProperty ReplyingCommentProperty =
            DependencyProperty.Register(nameof(ReplyingComment), typeof(Comment), typeof(ReplyCommentDialog), new PropertyMetadata(null));

        private void ReplyCommentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.tbContent.Text = "";
            if (ReplyingComment is null)
            {
                throw new InvalidOperationException();
            }

            try
            {
                this.pbLoading.IsIndeterminate = true;
                var reply = $"@{ReplyingComment.Author}{Environment.NewLine}";
                this.tbContent.Text = reply;
                this.tbContent.Select(reply.Length, 0);
            }
            finally
            {
                this.pbLoading.IsIndeterminate = false;
            }
            this.tbContent.Focus(FocusState.Programmatic);
        }

        private async void EditCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await ReplyingComment.Owner.PostCommentAsync(this.tbContent.Text);
            }
            catch (Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                this.pbLoading.IsIndeterminate = false;
            }
        }
    }
}
