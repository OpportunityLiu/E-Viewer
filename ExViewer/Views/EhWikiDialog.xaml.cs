using EhWikiClient;
using ExClient;
using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExViewer.Views
{
    public sealed partial class EhWikiDialog : ContentDialog
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
        }

        private string style;

        private IAsyncOperation<Record> loadRecord;

        internal async void SetTag(Tag tag)
        {
            loadRecord?.Cancel();
            wv.Visibility = Visibility.Collapsed;
            pb.Visibility = Visibility.Visible;
            var str = (string)null;
            Title = tag.Content;
            try
            {
                loadRecord = tag.FetchEhWikiRecordAsync();
                var record = await loadRecord;
                loadRecord = null;
                if(style == null)
                    initStyle();
                if(record?.DetialHtml == null)
                    str = Strings.Resources.Views.EhWikiDialog.TagNotFound;
                else
                    str = record.DetialHtml;
            }
            catch(Exception ex)
            {
                str = ex.GetMessage();
            }
            wv.NavigateToString(style + str);
            wv.Visibility = Visibility.Visible;
            pb.Visibility = Visibility.Collapsed;
        }

        private void initStyle()
        {
            var background = ((SolidColorBrush)this.Background);
            var foreground = ((SolidColorBrush)this.Foreground);
            var link = ((SolidColorBrush)this.BorderBrush);
            style = $@"
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
            return $"rgba({color.Color.R},{color.Color.G},{color.Color.B},{color.Color.A / 256d})";
        }

        private async void wv_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri;
            if(uri != null)
            {
                args.Cancel = true;
                if(uri.Host == "g.e-hentai.org")
                {
                    uri = new Uri(ExClient.Client.EhUri, uri.PathAndQuery + uri.Fragment);
                }
                await Launcher.LaunchUriAsync(uri);
            }
        }
    }
}
