using EhWikiClient;
using ExClient.Tagging;
using ExViewer.Controls;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
            s.refresh(s.WikiTag, null);
        }

        private string style;

        public string WikiTag
        {
            get => (string)GetValue(WikiTagProperty);
            set => SetValue(WikiTagProperty, value);
        }

        public static readonly DependencyProperty WikiTagProperty =
            DependencyProperty.Register(nameof(WikiTag), typeof(string), typeof(EhWikiDialog), new PropertyMetadata("", WikiTagPropertyChangedCallback));

        private static void WikiTagPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (EhWikiDialog)d;
            var o = (string)e.OldValue;
            var n = (string)e.NewValue;
            if (n.Equals(o))
                return;
            sender.refresh(n, o);
        }

        private static Regex regRedirect = new Regex(@"<div class=""redirectMsg""><p>Redirect to:</p><ul class=""redirectText""><li><a href=""/wiki/([^""]+)"" title=""([^""]+)"">([^<]+)</a></li></ul></div>", RegexOptions.Compiled);

        private static bool getRedirect(Record rec, out string redirectTitle)
        {
            redirectTitle = default;
            if (rec.DetialHtml.IsNullOrEmpty())
                return false;
            var redirect = regRedirect.Match(rec.DetialHtml);
            if (!redirect.Success)
                return false;

            redirectTitle = HtmlAgilityPack.HtmlEntity.DeEntitize(redirect.Groups[1].Value);
            return true;
        }

        private static readonly Dictionary<string, Record> cache = new Dictionary<string, Record>(StringComparer.OrdinalIgnoreCase);

        private static IAsyncOperation<Record> getRecord(string title)
        {
            if (cache.TryGetValue(title, out var r))
                return AsyncOperation<Record>.CreateCompleted(r);
            return AsyncInfo.Run(async token =>
            {
                var rc = await Client.FetchAsync(title).AsTask(token);
                cache[title] = rc;
                return rc;
            });
        }

        private int reinRefresh = 0;

        private async void refresh(string title, string previousTitle)
        {
            this.reinRefresh++;
            try
            {
                this.wv.Visibility = Visibility.Collapsed;
                this.pb.Visibility = Visibility.Visible;
                this.loadRecord?.Cancel();
                if (title.IsNullOrEmpty())
                {
                    this.wv.NavigateToString("");
                    return;
                }
                var str = (string)null;
                try
                {
                    this.loadRecord = getRecord(title);
                    var record = await this.loadRecord;
                    this.loadRecord = null;
                    if (record?.DetialHtml is null)
                    {
                        str = Strings.Resources.Views.EhWikiDialog.TagNotFound;
                    }
                    else if (getRedirect(record, out var rd))
                    {
                        this.wv.NavigateToString("");
                        if (!this.navigate(new Uri(eww, rd)) && !previousTitle.IsNullOrEmpty())
                            this.WikiTag = previousTitle;
                        return;
                    }
                    else
                    {
                        str = record.DetialHtml;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    str = $@"<p style='color:red;'>{HtmlAgilityPack.HtmlEntity.Entitize(ex.GetMessage(), true, true)}</p>";
                    Telemetry.LogException(ex);
                }
                if (this.style is null)
                {
                    this.initStyle();
                }

                this.wv.NavigateToString(this.style + str);
            }
            finally
            {
                if (--this.reinRefresh == 0)
                {
                    this.wv.Visibility = Visibility.Visible;
                    this.pb.Visibility = Visibility.Collapsed;
                }
            }
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
        private static readonly Uri eww = new Uri("https://ehwiki.org/wiki/");
        private static readonly char[] invalidTagChar = ":#/*%\"".ToCharArray();
        private static readonly HashSet<string> notTag = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Power",
            "Visual",
            "Contextual",
            "Costume",
            "Namespace",
            "Renaming",
            "Rewards",
            "Category",
            "Gallery Categories",
        };

        private static bool isValidTitle(string title)
        {
            return !notTag.Contains(title)
                        && title.IndexOfAny(invalidTagChar) < 0;
        }

        private bool navigate(Uri uri)
        {
            try
            {
                if (uri.Host == "g.e-hentai.org")
                {
                    uri = new Uri(eh, uri.PathAndQuery + uri.Fragment);
                    return false;
                }

                if (uri.Host != "ehwiki.org" ||
                    !uri.Fragment.IsNullOrEmpty() ||
                    !uri.AbsolutePath.StartsWith("/wiki/"))
                    return false;

                var tag = uri.AbsolutePath.Substring("/wiki/".Length);
                if (!isValidTitle(tag))
                    return false;

                this.WikiTag = tag.Replace('_', ' ');
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Ehwiki navigated", new Dictionary<string, string> { ["Tag"] = this.WikiTag });
                uri = null;
                return true;
            }
            finally
            {
                if (uri != null)
                {
                    var ignore = Launcher.LaunchUriAsync(uri);
                }
            }
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
