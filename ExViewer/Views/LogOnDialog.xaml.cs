﻿using ExClient;
using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

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
            if (pass != null)
            {
                pass.RetrievePassword();
                await this.wv.InvokeScriptAsync("eval", new[] { $@"
(function ()
{{
    var u = document.getElementsByName('UserName');
    if (u.length == 0) return;
    var nU = u[0];
    nU.value = '{escape(pass.UserName)}';
    var p = document.getElementsByName('PassWord');
    if (p.length == 0) return;
    var nP = p[0];
    nP.value = '{escape(pass.Password)}';
    var l = document.getElementsByName('LOGIN');
    if (l.length == 0) return;
    var nL = l[0];
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
            }
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
            System.Diagnostics.Debug.WriteLine(e.Uri.ToString(), "WebView");
            if (e.Uri.ToString().StartsWith(Client.LogOnUri.ToString()))
                await injectLogOnPage();
            else if (e.Uri.Host == Client.LogOnUri.Host)
                await injectOtherPage();
        }

        private LogOnInfo logOnInfoBackup;

        private string tempUserName, tempPassword;

        private bool hideCalled = false;

        private async void wv_ScriptNotify(object sender, NotifyEventArgs e)
        {
            if (this.hideCalled)
                return;
            var data = e.Value.Split('\n', StringSplitOptions.None);
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
            if (this.hideCalled)
                return;
            if (!string.IsNullOrEmpty(this.tempUserName) && !string.IsNullOrEmpty(this.tempPassword))
                AccountManager.CurrentCredential = AccountManager.CreateCredential(this.tempUserName, this.tempPassword);
            this.Hide();
            this.hideCalled = true;
        }
    }
}
