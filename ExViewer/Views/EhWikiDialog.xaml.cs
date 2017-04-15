using EhWikiClient;
using ExClient;
using ExViewer.Controls;
using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
            if(n.Equals(o))
                return;
            sender.refresh(n);
        }

        private async void refresh(Tag tag)
        {
            this.loadRecord?.Cancel();
            this.wv.Visibility = Visibility.Collapsed;
            this.pb.Visibility = Visibility.Visible;
            if(tag.Content == null)
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
                    if(record?.DetialHtml == null)
                        str = Strings.Resources.Views.EhWikiDialog.TagNotFound;
                    else
                        str = record.DetialHtml;
                }
                catch(System.Threading.Tasks.TaskCanceledException)
                {
                    return;
                }
                catch(Exception ex)
                {
                    str = ex.GetMessage();
                }
                if(this.style == null)
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
        background: {color(background)};
        font-family: sans-serif;
        font-size: 15px;
        color: {color(foreground)};
    }}
    
    a {{
        color:{color(link)}
    }}
    
    a:hover {{
        color: {color(foreground)}
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
<base href='https://ehwiki.org/'>";
        }

        private static string color(SolidColorBrush color)
        {
            if(color.Color.A == 255)
                return $"#{color.Color.R:X2}{color.Color.G:X2}{color.Color.B:X2}";
            return $"rgba({color.Color.R},{color.Color.G},{color.Color.B},{color.Color.A / 255d})";
        }

        private static readonly Uri eh = new Uri("https://e-hentai.org/");

        private async void wv_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri;
            if(uri != null)
            {
                args.Cancel = true;
                if(uri.Host == "g.e-hentai.org")
                {
                    uri = new Uri(eh, uri.PathAndQuery + uri.Fragment);
                }
                await Launcher.LaunchUriAsync(uri);
            }
        }
    }
}
