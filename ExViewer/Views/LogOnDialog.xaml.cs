using ExClient;
using ExViewer.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.Web.WebView2.Core;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
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
            keepAliveTimer.Tick += keepAliveTimer_Tick;
        }

        /// <summary>
        /// Keeps the forums connection/session warm while the user fills the log-on form.
        /// Idle connections are closed by the server and a later POST fails with
        /// ERR_CONNECTION_CLOSED.
        /// </summary>
        private readonly DispatcherTimer keepAliveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };

        private async void keepAliveTimer_Tick(object sender, object e)
        {
            if (VM.UseCookieLogOn || wv.CoreWebView2 is null)
            {
                keepAliveTimer.Stop();
                return;
            }
            try
            {
                await wv.ExecuteScriptAsync("fetch('/', { method: 'HEAD', cache: 'no-store' }).catch(function () {});");
            }
            catch
            {
                keepAliveTimer.Stop();
            }
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(VM.ShowErrorMsg) || e.PropertyName == nameof(VM.ErrorMsg))
            {
                if (VM.ShowErrorMsg && !string.IsNullOrEmpty(VM.ErrorMsg))
                {
                    // The cookie panel has its own error text.
                    if (VM.UseCookieLogOn)
                        return;
                    // Never replace the interactive log-on form with an error page;
                    // the user may still be typing.
                    var url = wv.CoreWebView2?.Source ?? "";
                    if (url.IndexOf("act=Login", StringComparison.OrdinalIgnoreCase) >= 0)
                        return;
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
                    var igneous = Igneous;

                    try
                    {
                        await Client.Current.LogOnAsync(uid, hash, igneous);
                    }
                    catch (Exception ex)
                    {
                        Succeed = false;
                        ErrorMsg = getErrorMessage(ex);
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
            private bool _UseCookieLogOn = true;
            public bool UseCookieLogOn { get => _UseCookieLogOn; set => Set(nameof(PrimaryButtonText), nameof(IsPrimaryButtonEnabled), ref _UseCookieLogOn, value); }

            public AsyncCommand LogOn { get; }

            public string PrimaryButtonText => _UseCookieLogOn
                ? Strings.Resources.Views.LogOnDialog.LogOnButtonText
                : Strings.Resources.Views.LogOnDialog.ResetButtonText;

            public bool IsPrimaryButtonEnabled => _UseCookieLogOn ? CanLogOn && !LogOn.IsExecuting : !LogOn.IsExecuting;

            public bool CanLogOn => !Succeed
                && long.TryParse(MemberId, out _)
                && Regex.IsMatch(PassHash ?? "", @"^[0-9a-fA-F]{32}$")
                && Regex.IsMatch(Igneous ?? "", @"^[0-9A-Za-z_-]*$");

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
            public string ErrorMsg { get => _ErrorMsg; set => Set(ref _ErrorMsg, value); }

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

        private static string getErrorMessage(Exception ex)
        {
            switch (ex.HResult)
            {
            case unchecked((int)0x80072EFD): // ERROR_INTERNET_CANNOT_CONNECT
            case unchecked((int)0x80072EE7): // ERROR_INTERNET_NAME_NOT_RESOLVED
            case unchecked((int)0x80072EE2): // ERROR_INTERNET_TIMEOUT
                return Strings.Resources.Views.LogOnDialog.NetworkError;
            default:
                return string.IsNullOrEmpty(ex.Message) ? ex.GetType().Name : ex.Message;
            }
        }

        public bool Succeed => VM.Succeed;

        private void btnUseCookie_Click(object sender, RoutedEventArgs e)
        {
            keepAliveTimer.Stop();
            VM.UseCookieLogOn = true;
            reset();
        }

        private void btnUseWebpage_Click(object sender, RoutedEventArgs e)
        {
            VM.UseCookieLogOn = false;
            reset();
        }

        private bool cookieLogOnTried;
        private bool cookieCheckInProgress;
        private bool exVisited;
        private long previousUserId;
        private string previousIgneous;

        private static readonly string[] cookieSources =
        {
            "https://forums.e-hentai.org/",
            "https://e-hentai.org/",
            "https://exhentai.org/",
        };

        private static readonly string[] trustedHosts =
        {
            "forums.e-hentai.org",
            "e-hentai.org",
            "exhentai.org",
        };

        private void reset()
        {
            keepAliveTimer.Stop();
            VM.Reset();

            VM.ShowErrorMsg = false;
            cookieLogOnTried = false;
            exVisited = false;
            if (VM.UseCookieLogOn)
                return;
            try
            {
                if (wv.CoreWebView2 is null)
                    wv.Source = Client.LogOnUri; // initializes CoreWebView2 and navigates
                else
                    wv.CoreWebView2.Navigate(Client.LogOnUri.ToString());
            }
            catch (Exception ex)
            {
                Telemetry.LogException(ex);
                VM.UseCookieLogOn = true;
                VM.ErrorMsg = Strings.Resources.Views.LogOnDialog.WebView2Unavailable;
                VM.ShowErrorMsg = true;
            }
        }

        private static bool IsTrustedHost(string host)
        {
            foreach (var trusted in trustedHosts)
            {
                if (string.Equals(host, trusted, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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
            var result = await wv.ExecuteScriptAsync($@"
(function ()
{{
    var nL = document.LOGIN;
    if(!nL) return false;
    var nU = nL.UserName;
    if(!nU) return false;
    var nP = nL.PassWord;
    if(!nP) return false;
    nU.value = '{u}';
    nP.value = '{p}';
    nL.addEventListener('submit', function ()
    {{
        window.chrome.webview.postMessage(nU.value + '\n' + nP.value);
    }});
    return true;
}})();
");
            if (result == "true" && !keepAliveTimer.IsEnabled)
                keepAliveTimer.Start();
            string escape(string value) => value
                .Replace(@"\", @"\\")
                .Replace("'", @"\'")
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\u2028", @"\u2028")
                .Replace("\u2029", @"\u2029");
        }

        private async Task tryLogOnFromCookiesAsync(Uri current)
        {
            if (cookieLogOnTried || cookieCheckInProgress || VM.LogOn.IsExecuting)
                return;

            cookieCheckInProgress = true;
            try
            {
                // Wait until the user actually submits the form and is redirected away
                // from the log-on page. Cookies may already exist from an older session.
                if (current.AbsoluteUri.IndexOf("act=Login", StringComparison.OrdinalIgnoreCase) >= 0)
                    return;

                var cookies = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var source in cookieSources)
                {
                    var list = await wv.CoreWebView2.CookieManager.GetCookiesAsync(source);
                    foreach (var cookie in list)
                    {
                        // An invalid value such as `igneous=mystery` from exhentai
                        // must not overwrite a valid value read from another domain.
                        if (cookie.Name == "igneous"
                            && !Client.IsValidIgneousValue(cookie.Value)
                            && cookies.TryGetValue("igneous", out var currentIgneous)
                            && Client.IsValidIgneousValue(currentIgneous))
                        {
                            continue;
                        }
                        cookies[cookie.Name] = cookie.Value;
                    }
                }

                if (!cookies.TryGetValue("ipb_member_id", out var memberId)
                    || !cookies.TryGetValue("ipb_pass_hash", out var passHash)
                    || string.IsNullOrWhiteSpace(memberId)
                    || string.IsNullOrWhiteSpace(passHash))
                    return;

                cookies.TryGetValue("igneous", out var igneous);
                var igneousUsable = Client.IsValidIgneousValue(igneous);

                // A web log-on cannot create a valid igneous on its own. If the app
                // already had one for the same account before this dialog opened,
                // reuse it.
                if (!igneousUsable
                    && long.TryParse(memberId, out var userId)
                    && userId == previousUserId
                    && Client.IsValidIgneousValue(previousIgneous))
                {
                    igneous = previousIgneous;
                    igneousUsable = true;
                }

                // Without igneous, let the browser visit exhentai once; the site may
                // hand out a fresh one for the current session.
                if (!igneousUsable
                    && Client.Current.Host == HostType.ExHentai
                    && !exVisited)
                {
                    exVisited = true;
                    wv.CoreWebView2.Navigate("https://exhentai.org/");
                    return;
                }

                cookieLogOnTried = true;
                VM.MemberId = memberId;
                VM.PassHash = passHash;
                VM.Igneous = igneousUsable ? igneous : "";

                if (!igneousUsable && Client.Current.Host == HostType.ExHentai)
                {
                    // Web log-on succeeded, but this network cannot obtain a valid
                    // igneous. Switch to the cookie form with the other cookies filled.
                    keepAliveTimer.Stop();
                    VM.UseCookieLogOn = true;
                    VM.ErrorMsg = Strings.Resources.Views.LogOnDialog.IgneousNotAvailable;
                    VM.ShowErrorMsg = true;
                    return;
                }

                VM.LogOn.Execute();
            }
            finally
            {
                cookieCheckInProgress = false;
            }
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
            // Capture the previous account's igneous before the cookies are cleared;
            // it can be reused if the web log-on cannot provide a valid one.
            previousUserId = Client.Current.UserId;
            previousIgneous = null;
            Client.Current.TryGetValidIgneousFor(previousUserId, out previousIgneous);
            Client.Current.ClearLogOnInfo();

            reset();
        }

        private void wv_CoreWebView2Initialized(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.UI.Xaml.Controls.CoreWebView2InitializedEventArgs args)
        {
            if (args.Exception == null)
                return;

            keepAliveTimer.Stop();
            Telemetry.LogException(args.Exception);
            VM.UseCookieLogOn = true;
            VM.ErrorMsg = Strings.Resources.Views.LogOnDialog.WebView2Unavailable;
            VM.ShowErrorMsg = true;
        }

        private async void wv_NavigationCompleted(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            try
            {
                await handleNavigationCompletedAsync(sender, args);
            }
            catch (Exception ex)
            {
                keepAliveTimer.Stop();
                Telemetry.LogException(ex);
                if (!VM.UseCookieLogOn)
                    showErrorMsgInWv(Strings.Resources.Views.LogOnDialog.WebViewConnectionError, sender.Source?.ToString() ?? "");
            }
        }

        private async Task handleNavigationCompletedAsync(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess)
            {
                string message;
                switch (args.WebErrorStatus)
                {
                // Cloudflare challenge navigations are often superseded by a new
                // navigation; treat these as transient instead of killing the page.
                case CoreWebView2WebErrorStatus.Unknown:
                case CoreWebView2WebErrorStatus.OperationCanceled:
                case CoreWebView2WebErrorStatus.ConnectionAborted:
                case CoreWebView2WebErrorStatus.ConnectionReset:
                    return;
                case CoreWebView2WebErrorStatus.CannotConnect:
                case CoreWebView2WebErrorStatus.Timeout:
                case CoreWebView2WebErrorStatus.ServerUnreachable:
                case CoreWebView2WebErrorStatus.HostNameNotResolved:
                    message = Strings.Resources.Views.LogOnDialog.WebViewConnectionError;
                    break;
                default:
                    message = $"{args.WebErrorStatus} ({(int)args.WebErrorStatus})";
                    break;
                }
                showErrorMsgInWv(message, sender.Source?.ToString() ?? "");
                return;
            }

            var uri = sender.Source;
            Debug.WriteLine(uri?.ToString() ?? "local string", "WebView2");
            if (uri is null || !IsTrustedHost(uri.Host))
                return;

            if (uri.AbsoluteUri.StartsWith(Client.LogOnUri.AbsoluteUri, StringComparison.Ordinal))
                await injectLogOnPage();

            await tryLogOnFromCookiesAsync(uri);
        }

        private void showErrorMsgInWv(string line1, string line2)
        {
            if (VM.UseCookieLogOn)
                return;
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

        private void wv_WebMessageReceived(Microsoft.UI.Xaml.Controls.WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            // Only accept messages posted by the forums log-on page.
            if (!Uri.TryCreate(args.Source, UriKind.Absolute, out var source)
                || source.Scheme != Uri.UriSchemeHttps
                || !string.Equals(source.Host, Client.LogOnUri.Host, StringComparison.OrdinalIgnoreCase))
                return;
            string value;
            try
            {
                value = args.TryGetWebMessageAsString();
            }
            catch
            {
                return;
            }
            if (string.IsNullOrEmpty(value))
                return;
            var data = value.Split(new[] { '\n' }, StringSplitOptions.None);
            if (data.Length != 2)
                return;
            VM.UserName = data[0];
            VM.Password = data[1];
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

        private void cd_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            keepAliveTimer.Stop();
            try
            {
                wv.Close();
            }
            catch
            {
            }
        }
    }
}
