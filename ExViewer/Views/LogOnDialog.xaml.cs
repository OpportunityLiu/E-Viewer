using ExClient;
using ExViewer.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
            InitializeComponent();
            VM.PropertyChanged += VM_PropertyChanged;
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(VM.ShowErrorMsg) || e.PropertyName == nameof(VM.ErrorMsg))
            {
                if (VM.ShowErrorMsg && !string.IsNullOrEmpty(VM.ErrorMsg))
                {
                    showErrorMsgInWv(VM.ErrorMsg, "");
                }
            }
        }

        private class VMData : ObservableObject
        {
            public VMData()
            {
                LogOn = AsyncCommand.Create(async s =>
                {
                    if (!long.TryParse(MemberId, out var uid))
                        return;
                    var hash = PassHash;
                    var igenous = Igneous;

                    try
                    {
                        await Client.Current.LogOnAsync(uid, hash, igenous);
                    }
                    catch (Exception ex)
                    {
                        Reset();
                        ErrorMsg = ex.Message;
                        ShowErrorMsg = true;
                        Telemetry.LogException(ex);
                        return;
                    }
                    if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                        AccountManager.CurrentCredential = AccountManager.CreateCredential(UserName, Password);

                    Succeed = true;
                }, s => CanLogOn);
                LogOn.PropertyChanged += (s, e) => OnPropertyChanged(nameof(IsPrimaryButtonEnabled));
            }

            public LogOnInfo LogOnInfoBackup { get; set; }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool _UseCookieLogOn;
            public bool UseCookieLogOn { get => _UseCookieLogOn; set => Set(nameof(PrimaryButtonText), nameof(IsPrimaryButtonEnabled), ref _UseCookieLogOn, value); }

            public AsyncCommand LogOn { get; }

            public string PrimaryButtonText => _UseCookieLogOn
                ? Strings.Resources.Views.LogOnDialog.LogOnButtonText
                : Strings.Resources.Views.LogOnDialog.ResetButtonText;

            public bool IsPrimaryButtonEnabled => _UseCookieLogOn ? CanLogOn && !LogOn.IsExecuting : !LogOn.IsExecuting;

            public bool CanLogOn => !Succeed
                && long.TryParse(MemberId, out _)
                && Regex.IsMatch(PassHash ?? "", @"^[0-9a-fA-F]{32}$")
                && Regex.IsMatch(Igneous ?? "", @"^[0-9a-fA-F]{0,32}$");

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool _Succeed;
            public bool Succeed
            {
                get => _Succeed;
                set => Set(nameof(CanLogOn), nameof(IsPrimaryButtonEnabled), ref _Succeed, value);
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _UserName;
            public string UserName { get => _UserName; set => Set(ref _UserName, value); }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _Password;
            public string Password { get => _Password; set => Set(ref _Password, value); }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _MemberId;
            public string MemberId
            {
                get => _MemberId;
                set => Set(nameof(CanLogOn), nameof(IsPrimaryButtonEnabled), ref _MemberId, value);
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _PassHash;
            public string PassHash
            {
                get => _PassHash;
                set => Set(nameof(CanLogOn), nameof(IsPrimaryButtonEnabled), ref _PassHash, value);
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _Igneous;
            public string Igneous
            {
                get => _Igneous;
                set => Set(nameof(CanLogOn), nameof(IsPrimaryButtonEnabled), ref _Igneous, value);
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string _ErrorMsg;
            public string ErrorMsg { get => _ErrorMsg; private set => Set(ref _ErrorMsg, value); }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private bool _ShowErrorMsg;
            public bool ShowErrorMsg { get => _ShowErrorMsg; set => Set(ref _ShowErrorMsg, value); }


            public void Reset()
            {
                Succeed = false;

                UserName = null;
                Password = null;

                MemberId = "";
                PassHash = "";
                Igneous = "";
            }
        }

        private readonly VMData VM = new VMData();

        public bool Succeed => VM.Succeed;

        private void btnUseCookie_Click(object sender, RoutedEventArgs e)
        {
            VM.UseCookieLogOn = true;
            reset();
        }

        private void btnUseWebpage_Click(object sender, RoutedEventArgs e)
        {
            VM.UseCookieLogOn = false;
            reset();
        }

        private async void reset()
        {
            VM.Reset();

            VM.ShowErrorMsg = false;
            wv.NavigateToString("");
            await Dispatcher.YieldIdle();
            wv.Navigate(Client.LogOnUri);
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
            await wv.InvokeScriptAsync("eval", new[] { $@"
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
            string escape(string value) => value.Replace(@"\", @"\\").Replace("'", @"\'");
        }

        private async Task injectOtherPage()
        {
            if (VM.LogOn.IsExecuting)
                return;

            var r = await wv.InvokeScriptAsync("eval", new[] { @"
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
            VM.MemberId = data[0];
            VM.PassHash = data[1];
            VM.Igneous = "";
            wv.NavigateToString("");
            VM.LogOn.Execute();
        }

        private void cd_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void cd_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (Client.Current.NeedLogOn)
                CloseButtonText = Strings.Resources.General.Exit;
            else
                CloseButtonText = Strings.Resources.General.Cancel;

            VM.LogOnInfoBackup = Client.Current.GetLogOnInfo();
            Client.Current.ClearLogOnInfo();

            reset();
        }

        private async void wv_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            Debug.WriteLine(args.Uri?.ToString() ?? "local string", "WebView");
            if (args.Uri is null)
                return;

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

        private void showErrorMsgInWv(string line1, string line2)
        {
            wv.NavigateToString($@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' />
</head>
<body style='background:{Color((SolidColorBrush)Background)}; font-family: sans-serif;'>
    <div>
        <p style='color:red;white-space:pre-wrap;'>{System.Net.WebUtility.HtmlEncode(line1)}</p>
        <p>
            <small style='color:{Color((SolidColorBrush)Foreground)};white-space:pre-wrap;'>{System.Net.WebUtility.HtmlEncode(line2)}</small>
        </p>
    </div>
</body>
</html>");

        }

        private void wv_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            showErrorMsgInWv($"{System.Net.WebUtility.HtmlEncode(e.WebErrorStatus.ToString())} ({(int)e.WebErrorStatus})", e.Uri.ToString());
        }

        private void wv_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var data = e.Value.Split(new[] { '\n' }, StringSplitOptions.None);
            if (e.CallingUri.ToString().StartsWith(Client.LogOnUri.ToString()))
            {
                if (data.Length != 2)
                {
                    return;
                }

                VM.UserName = data[0];
                VM.Password = data[1];
                return;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var ww = Window.Current.Bounds.Width;
            if (ww > 400)
                wv.MinWidth = Math.Min(availableSize.Width - 144, 700);
            else
                wv.MinWidth = 0;

            return base.MeasureOverride(availableSize);
        }

        private void cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (VM.UseCookieLogOn)
            {
                VM.LogOn.Execute();
            }
            else
            {
                reset();
            }
        }

        private void cookie_TextChanged(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text != "")
                VM.ShowErrorMsg = false;
        }

        private void cd_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Succeed)
                Hide();
        }

        private void cd_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (VM.LogOn.IsExecuting)
            {
                args.Cancel = true;
                return;
            }
            if (Succeed)
                return;
            if (VM.LogOnInfoBackup != null)
                Client.Current.RestoreLogOnInfo(VM.LogOnInfoBackup);
            if (Client.Current.NeedLogOn)
                Application.Current.Exit();
        }
    }
}
