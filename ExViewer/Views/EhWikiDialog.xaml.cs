using EhWikiClient;
using ExClient;
using ExClient.Tagging;
using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static ExViewer.Helpers.HtmlHelper;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExViewer.Views
{
    public sealed partial class EhWikiDialog : MyContentDialog
    {
        public EhWikiDialog()
        {
            this.InitializeComponent();
            this.RegisterPropertyChangedCallback(RequestedThemeProperty, requestedThemeChanged);
        }

        private static void requestedThemeChanged(DependencyObject sender, DependencyProperty dp)
        {
            var s = (EhWikiDialog)sender;
            s.style = null;
            s.refresh(s.WikiTag);
        }

        private string style;

        public Tag WikiTag
        {
            get => (Tag)GetValue(WikiTagProperty);
            set => SetValue(WikiTagProperty, value);
        }

        public static readonly DependencyProperty WikiTagProperty =
            DependencyProperty.Register(nameof(WikiTag), typeof(Tag), typeof(EhWikiDialog), new PropertyMetadata(default(Tag), WikiTagPropertyChangedCallback));

        private static void WikiTagPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (EhWikiDialog)d;
            var o = (Tag)e.OldValue;
            var n = (Tag)e.NewValue;
            if (n.Equals(o))
                return;
            sender.refresh(n);
        }

        private static Regex regRedirect = new Regex(@"<div class=""redirectMsg""><p>Redirect to:</p><ul class=""redirectText""><li><a href=""([^""]+)"" title=""([^""]+)"">([^<]+)</a></li></ul></div>", RegexOptions.Compiled);

        private async void refresh(Tag tag)
        {
            this.loadRecord?.Cancel();
            this.wv.Visibility = Visibility.Collapsed;
            this.pb.Visibility = Visibility.Visible;
            if (tag.Content == null)
            {
                this.Title = "";
                this.wv.NavigateToString("");
            }
            else
            {
                var str = (string)null;
                this.Title = tag.Content;
                try
                {
                    this.loadRecord = tag.FetchEhWikiRecordAsync();
                    var record = await this.loadRecord;
                    this.loadRecord = null;
                    if (record?.DetialHtml == null)
                        str = Strings.Resources.Views.EhWikiDialog.TagNotFound;
                    else
                        str = record.DetialHtml;
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    str = $@"<p style='color:red;'>{HtmlAgilityPack.HtmlEntity.Entitize(ex.GetMessage(), true, true)}</p>";
                }
                var redirect = regRedirect.Match(str);
                if (redirect.Success)
                {
                    this.navigate(new Uri(ew, HtmlAgilityPack.HtmlEntity.DeEntitize(redirect.Groups[1].Value)));
                }
                if (this.style == null)
                    this.initStyle();
                this.wv.NavigateToString(this.style + str);

            }
            this.wv.Visibility = Visibility.Visible;
            this.pb.Visibility = Visibility.Collapsed;
        }

        private IAsyncOperation<Record> loadRecord;

        private void initStyle()
        {
            var background = ((SolidColorBrush)this.Background);
            var foreground = ((SolidColorBrush)this.Foreground);
            var link = ((SolidColorBrush)this.BorderBrush);
            this.style = $@"
<meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
<style type='text/css'>
    html {{
        background: {Color(background)};
        font-family: sans-serif;
        font-size: 15px;
        color: {Color(foreground)};
    }}
    
    a {{
        color:{Color(link)}
    }}
    
    a:hover {{
        color: {Color(foreground)}
    }}
    
    ul {{
        margin: 0px;
        padding: 0px;
        padding-left: 20px;
    }}
    
    li {{
        margin-top: 4px;
        margin-bottom: 4px;
        list-style-type: square;
    }}
    
    dd {{
        margin: 0px;
        margin-left: 20px;
    }}
    
    dl {{
        margin-top: 4px;
        margin-bottom: 4px;
    }}
</style>
<base href='{ew.ToString()}'>";
        }

        private static readonly Uri eh = new Uri("https://e-hentai.org/");
        private static readonly Uri ew = new Uri("https://ehwiki.org/");
        private static readonly char[] invalidTagChar = ":/*%\"".ToCharArray();
        private static readonly HashSet<string> notTag = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Power",
            "Visual",
            "Contextual",
            "Costume",
            "Namespace",
            "Genderbend"
        };

        private async void navigate(Uri uri)
        {
            if (uri.Host == "ehwiki.org" && string.IsNullOrEmpty(uri.Fragment))
            {
                if (uri.AbsolutePath.StartsWith("/wiki/"))
                {
                    var tag = uri.AbsolutePath.Substring(6);
                    if (!notTag.Contains(tag)
                        && tag.IndexOfAny(invalidTagChar) < 0)
                    {
                        this.WikiTag = new Tag(Namespace.Misc, tag.Replace('_', ' '));
                        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Ehwiki navigated", new Dictionary<string, string> { ["Tag"] = this.WikiTag.ToString() });
                        return;
                    }
                }
            }
            if (uri.Host == "g.e-hentai.org")
            {
                uri = new Uri(eh, uri.PathAndQuery + uri.Fragment);
            }
            await Launcher.LaunchUriAsync(uri);
        }

        private void wv_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri;
            if (uri != null)
            {
                args.Cancel = true;
                navigate(uri);
            }
        }

        private void MyContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Ehwiki opened", new Dictionary<string, string> { ["Tag"] = this.WikiTag.ToString() });
        }
    }
}
