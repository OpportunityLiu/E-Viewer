using ExClient;
using ExViewer.Controls;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static ExViewer.Helpers.HtmlHelper;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace ExViewer.Views
{
    public sealed partial class LogOnDialog : MyContentDialog
    {
        public LogOnDialog()
        {
            this.InitializeComponent();
        }

        private async void reset()
        {
            this.tempUserName = null;
            this.tempPassword = null;
            this.wv.NavigateToString($@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
</head>
<body style='background:{Color((SolidColorBrush)this.Background)};'>
</body>
</html>");
            await Dispatcher.YieldIdle();
            this.wv.Navigate(Client.LogOnUri);
        }

        private async Task injectLogOnPage()
        {
            var pass = AccountManager.CurrentCredential;
            var u = "";
            var p = "";
            if (pass != null)
            {
                pass.RetrievePassword();
                u = escape(pass.UserName);
                p = escape(pass.Password);
            }
            await this.wv.InvokeScriptAsync("eval", new[] { $@"
(function ()
{{
    var nL = document.LOGIN;
    if(!nL) return;
	var nU = nL.UserName;
    if(!nU) return;
    var nP = nL.PassWord;
    if(!nP) return;
    nU.value = '{u}';
    nP.value = '{p}';
    nL.onsubmit = function(ev)
    {{
        var ret = ValidateForm();
        if (ret)
        {{
            window.external.notify(nU.value + '\n' + nP.value);
        }}
        return ret;
    }}
}})();
" });
            string escape(string value)
            {
                return value.Replace(@"\", @"\\").Replace("'", @"\'");
            }
        }

        private async Task injectOtherPage()
        {
            if (this.loggingOn)
            {
                return;
            }

            var r = await this.wv.InvokeScriptAsync("eval", new[] { @"
(function ()
{
    function getCookie(c_name)
    {
        if (document.cookie.length <= 0) return '';
        var c_start = document.cookie.indexOf(c_name + '=');
        if (c_start < 0) return '';
        c_start = c_start + c_name.length + 1;
        var c_end = document.cookie.indexOf(';', c_start);
        if (c_end == -1) c_end = document.cookie.length;
        return unescape(document.cookie.substring(c_start, c_end));
    }
    return (getCookie('ipb_member_id') + '\n' + getCookie('ipb_pass_hash'));
})();
" });
            var data = r.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
            {
                return;
            }

            await logOnAsync(data[0], data[1]);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Client.Current.NeedLogOn)
            {
                this.CloseButtonText = Strings.Resources.General.Exit;
            }
            else
            {
                this.CloseButtonText = Strings.Resources.General.Cancel;
            }
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.logOnInfoBackup = Client.Current.GetLogOnInfo();
            reset();
        }

        private async void wv_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.Uri?.ToString() ?? "local string", "WebView");
            if (args.Uri is null)
            {
                return;
            }

            if (args.Uri == Client.LogOnUri)
            {
                await injectLogOnPage();
            }
            else if (args.Uri.ToString().StartsWith(Client.LogOnUri.ToString()))
            {
                await injectLogOnPage();
                await injectOtherPage();
            }
            else if (args.Uri.Host == Client.LogOnUri.Host)
            {
                await injectOtherPage();
            }
        }

        private void wv_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            this.wv.NavigateToString($@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
</head>
<body style='background:{Color((SolidColorBrush)this.Background)}; font-family: sans-serif;'>
    <div>
        <p style='color:red;'>
            {(int)e.WebErrorStatus} ({e.WebErrorStatus.ToString()})
        </p>
        <small style='color:{Color((SolidColorBrush)this.Foreground)}'>
            {e.Uri}
        </small>
    </div>
</body>
</html>");
        }

        private LogOnInfo logOnInfoBackup;

        private string tempUserName, tempPassword;

        private void wv_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var data = e.Value.Split(new[] { '\n' }, StringSplitOptions.None);
            if (e.CallingUri.ToString().StartsWith(Client.LogOnUri.ToString()))
            {
                if (data.Length != 2)
                {
                    return;
                }

                this.tempUserName = data[0];
                this.tempPassword = data[1];
                return;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var ww = Window.Current.Bounds.Width;
            if (ww > 400)
            {
                this.wv.MinWidth = Math.Min(availableSize.Width - 144, 700);
            }
            else
            {
                this.wv.MinWidth = 0;
            }

            return base.MeasureOverride(availableSize);
        }

        private void MyContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            reset();
            args.Cancel = true;
        }

        private void MyContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (this.logOnInfoBackup != null)
            {
                Client.Current.RestoreLogOnInfo(this.logOnInfoBackup);
            }

            if (Client.Current.NeedLogOn)
            {
                Application.Current.Exit();
            }
        }

        private bool loggingOn = false;
        private async Task logOnAsync(string id, string hash)
        {
            this.loggingOn = true;
            try
            {
                if (!long.TryParse(id, out var uid))
                {
                    return;
                }

                try
                {
                    await Client.Current.LogOnAsync(uid, hash);
                }
                catch (Exception)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(this.tempUserName) && !string.IsNullOrEmpty(this.tempPassword))
                {
                    AccountManager.CurrentCredential = AccountManager.CreateCredential(this.tempUserName, this.tempPassword);
                }

                this.Hide();
            }
            finally
            {
                this.loggingOn = false;
            }
        }
    }
}
