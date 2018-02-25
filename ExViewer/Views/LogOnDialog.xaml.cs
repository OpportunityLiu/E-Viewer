using ExClient;
using ExViewer.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void reset()
        {
            this.wv.Navigate(Client.LogOnUri);
            this.tempUserName = null;
            this.tempPassword = null;
            this.hideCalled = false;
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
            await this.wv.InvokeScriptAsync("eval", new[] { @"
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
    window.external.notify('\n' + getCookie('ipb_member_id') + '\n' + getCookie('ipb_pass_hash'));
})();
" });
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Secondary)
            {
                if (this.logOnInfoBackup != null)
                    Client.Current.RestoreLogOnInfo(this.logOnInfoBackup);
                if (Client.Current.NeedLogOn)
                {
                    Application.Current.Exit();
                }
            }
            else
            {
                if (!this.hideCalled)
                    args.Cancel = true;
            }
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Client.Current.NeedLogOn)
                this.SecondaryButtonText = Strings.Resources.General.Exit;
            else
                this.SecondaryButtonText = Strings.Resources.General.Cancel;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.logOnInfoBackup = Client.Current.GetLogOnInfo();
            reset();
        }

        private async void wv_LoadCompleted(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Uri?.ToString() ?? "local string", "WebView");
            if (e.Uri == null)
                return;
            if (e.Uri.ToString().StartsWith(Client.LogOnUri.ToString()))
                await injectLogOnPage();
            else if (e.Uri.Host == Client.LogOnUri.Host)
                await injectOtherPage();
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

        private bool hideCalled = false;

        private async void wv_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (this.hideCalled)
                return;
            var data = e.Value.Split(new[] { '\n' }, StringSplitOptions.None);
            if (e.CallingUri.ToString().StartsWith(Client.LogOnUri.ToString()))
            {
                if (data.Length != 2)
                    return;
                this.tempUserName = data[0];
                this.tempPassword = data[1];
                return;
            }
            if (data.Length != 3 || data[0].Length != 0)
                return;
            await logOnAsync(data[1], data[2]);
        }

        private void MyContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (this.hideCalled)
                return;
            reset();
        }

        private async Task logOnAsync(string id, string hash)
        {
            if (!long.TryParse(id, out var uid))
                return;
            if (this.hideCalled)
                return;
            try
            {
                await Client.Current.LogOnAsync(uid, hash);
            }
            catch (Exception)
            {
                return;
            }
            if (!string.IsNullOrEmpty(this.tempUserName) && !string.IsNullOrEmpty(this.tempPassword))
                AccountManager.CurrentCredential = AccountManager.CreateCredential(this.tempUserName, this.tempPassword);
            if (this.hideCalled)
                return;
            this.hideCalled = true;
            this.Hide();
        }
    }
}
