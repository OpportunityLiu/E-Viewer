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
            InitializeComponent();
        }

        private void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            tbInfo.Text = "";
        }

        private void tbContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbInfo.Text = "";
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
            if (tbContent.IsReadOnly)
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
            var text = tbContent.Text;
            var sS = tbContent.SelectionStart;
            var currentSelected = tbContent.SelectedText;
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
                tbContent.Select(sS, currentSelected.Length + begin.Length);
                currentSelected = tbContent.SelectedText;
            }
            if (currentSelected.EndsWith(end))
            {
                endsWith = true;
            }
            else if (safeSubstring(text, sS + currentSelected.Length, end.Length) == end)
            {
                endsWith = null;
                tbContent.Select(sS, currentSelected.Length + end.Length);
                currentSelected = tbContent.SelectedText;
            }
            if (startsWith is null && endsWith == false)
            {
                startsWith = false;
                sS += begin.Length;
                tbContent.Select(sS, currentSelected.Length - begin.Length);
                currentSelected = tbContent.SelectedText;
            }
            else if (startsWith == false && endsWith is null)
            {
                endsWith = false;
                tbContent.Select(sS, currentSelected.Length - end.Length);
                currentSelected = tbContent.SelectedText;
            }
            if (startsWith != false && endsWith != false)
            {
                tbContent.SelectedText = currentSelected.Substring(begin.Length, currentSelected.Length - begin.Length - end.Length);
                return;
            }
            var replaced = string.Concat(begin, currentSelected, end);
            tbContent.SelectedText = replaced;
            var s = tbContent.SelectionStart;
            if (currentSelected.Length == 0)
            {
                tbContent.Select(s + replaced.Length - end.Length, 0);
            }
        }

        private static Regex urlRegex = new Regex(@"^\[url(?:=[^\]]*?)?\](.*)\[/url\]$", RegexOptions.Compiled);

        private void handleUrl()
        {
            var currentSelected = tbContent.SelectedText;
            var end = "[/url]";
            var match = urlRegex.Match(currentSelected);
            if (match.Success)
            {
                tbContent.SelectedText = match.Groups[1].Value;
                return;
            }
            if (Uri.TryCreate(currentSelected, UriKind.Absolute, out var uri) &&
                (uri.Host.EndsWith("e-hentai.org") || uri.Host.EndsWith("exhentai.org")))
            {
                var begin = $"[url={currentSelected}]";
                var replaced = string.Concat(begin, currentSelected, end);
                tbContent.SelectedText = replaced;
                var s = tbContent.SelectionStart;
                tbContent.Select(s + begin.Length, currentSelected.Length);
            }
            else
            {
                const string begin = "[url]";
                var replaced = string.Concat(begin, currentSelected, end);
                tbContent.SelectedText = replaced;
                var s = tbContent.SelectionStart;
                if (currentSelected.Length == 0)
                {
                    tbContent.Select(s + replaced.Length - end.Length, 0);
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
                ctrlDown = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Control ||
                e.OriginalKey == Windows.System.VirtualKey.LeftControl ||
                e.OriginalKey == Windows.System.VirtualKey.RightControl)
            {
                ctrlDown = false;
            }
            base.OnKeyUp(e);
            if (e.Handled || !ctrlDown)
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
                gdEditBar.HorizontalAlignment = HorizontalAlignment.Right;
                gdEditBar.Width = 320;
            }
            else
            {
                gdEditBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                gdEditBar.Width = double.NaN;
            }
        }
    }

    public sealed class AddCommentDialog : CommentDialog
    {
        public AddCommentDialog()
        {
            Title = Strings.Resources.Views.CommentDialog.AddTitle;
            PrimaryButtonClick += AddCommentDialog_PrimaryButtonClick;
            Opened += AddCommentDialog_Opened;
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
                throw new InvalidOperationException("Gallery is null");

            tbContent.Text = "";
            tbContent.Focus(FocusState.Programmatic);
        }

        private async void AddCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await Gallery.Comments.PostCommentAsync(tbContent.Text);
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                pbLoading.IsIndeterminate = false;
            }
        }
    }

    public sealed class EditCommentDialog : CommentDialog
    {
        public EditCommentDialog()
        {
            Title = Strings.Resources.Views.CommentDialog.EditTitle;
            Opened += EditCommentDialog_Opened;
            PrimaryButtonClick += EditCommentDialog_PrimaryButtonClick;
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
            tbContent.Text = "";
            if (EditableComment is null)
                throw new InvalidOperationException("EditableComment is null");
            try
            {
                pbLoading.IsIndeterminate = true;
                var editable = await EditableComment.FetchEditableAsync() ?? "";
                tbContent.Text = editable;
                tbContent.Select(editable.Length, 0);
            }
            catch (Exception ex)
            {
                tbContent.Text = "";
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                pbLoading.IsIndeterminate = false;
            }
            tbContent.Focus(FocusState.Programmatic);
        }

        private async void EditCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await EditableComment.EditAsync(tbContent.Text);
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                pbLoading.IsIndeterminate = false;
            }
        }
    }

    public sealed class ReplyCommentDialog : CommentDialog
    {
        public ReplyCommentDialog()
        {
            Title = Strings.Resources.Views.CommentDialog.AddTitle;
            Opened += ReplyCommentDialog_Opened;
            PrimaryButtonClick += EditCommentDialog_PrimaryButtonClick;
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
            tbContent.Text = "";
            if (ReplyingComment is null)
                throw new InvalidOperationException("ReplyingComment is null");
            try
            {
                pbLoading.IsIndeterminate = true;
                var reply = $"@{ReplyingComment.Author}{Environment.NewLine}";
                tbContent.Text = reply;
                tbContent.Select(reply.Length, 0);
            }
            finally
            {
                pbLoading.IsIndeterminate = false;
            }
            tbContent.Focus(FocusState.Programmatic);
        }

        private async void EditCommentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            pbLoading.IsIndeterminate = true;
            var d = args.GetDeferral();
            try
            {
                await ReplyingComment.Owner.PostCommentAsync(tbContent.Text);
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
                args.Cancel = true;
            }
            finally
            {
                d.Complete();
                pbLoading.IsIndeterminate = false;
            }
        }
    }
}
