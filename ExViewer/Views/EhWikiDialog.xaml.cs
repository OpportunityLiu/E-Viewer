using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ExClient;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;

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

        internal async void SetTag(Tag tag)
        {
            var str = (string)null;
            Title = tag.Content;
            try
            {
                var record = await tag.FetchEhWikiRecordAsync();
                if(style == null)
                    initStyle();
                if(record?.Html == null)
                    str = "Tag not fount in wiki.";
                else
                    str = record.Html;
            }
            catch(Exception ex)
            {
                str = ex.GetMessage();
            }
            wv.NavigateToString(style + str);
        }

        private void initStyle()
        {
            var background = ((SolidColorBrush)this.Background);
            var foreground = ((SolidColorBrush)this.Foreground);
            var link = ((SolidColorBrush)this.BorderBrush);
            style = $@"<meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
<style type='text/css'>
	html {{
		background: {rgba(background)};
		font-family: sans-serif;
		font-size: 15px;
		color: {rgba(foreground)};
	}}
	
	a {{
		color:{rgba(link)}
	}}
	
	a:hover {{
		color: {rgba(foreground)}
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

        private static string rgba(SolidColorBrush color)
        {
            return $"rgba({color.Color.R},{color.Color.G},{color.Color.B},{color.Color.A})";
        }

        private async void wv_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if(args.Uri != null)
            {
                args.Cancel = true;
                await Launcher.LaunchUriAsync(args.Uri);
            }
        }
    }
}
