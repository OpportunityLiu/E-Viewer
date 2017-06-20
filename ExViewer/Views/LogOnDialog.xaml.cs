using ExClient;
using ExViewer.Controls;
using System;
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
            this.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
        }

        ReCaptcha recap;

        private async Task loadReCapcha()
        {
            this.sp_ReCaptcha.Visibility = Visibility.Visible;
            this.img_ReCaptcha.Source = null;
            this.tb_ReCaptcha.Text = "";
            try
            {
                this.recap = await ReCaptcha.FetchAsync();
                this.img_ReCaptcha.Source = new BitmapImage(this.recap.ImageUri);
            }
            catch(Exception ex)
            {
                this.tb_info.Text = ex.GetMessage();
            }
        }

        private async Task logOnAsync(ContentDialogClosingEventArgs args)
        {
            try
            {
                var username = this.tb_user.Text;
                var password = this.pb_pass.Password;
                if(string.IsNullOrWhiteSpace(username))
                {
                    this.tb_info.Text = Strings.Resources.Views.LogOnDialog.NoUserName;
                    this.tb_user.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                else if(string.IsNullOrEmpty(password))
                {
                    this.tb_info.Text = Strings.Resources.Views.LogOnDialog.NoPassword;
                    this.pb_pass.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                else
                {
                    var d = args.GetDeferral();
                    try
                    {
                        this.tb_info.Text = "";
                        try
                        {
                            if(this.recap != null)
                                await this.recap.Submit(this.tb_ReCaptcha.Text);
                        }
                        catch(Exception ex)
                        {
                            await loadReCapcha();
                            this.tb_info.Text = ex.GetMessage();
                            this.tb_ReCaptcha.Focus(FocusState.Programmatic);
                            args.Cancel = true;
                            return;
                        }
                        try
                        {
                            await Client.Current.LogOnAsync(username, password, this.recap);
                            AccountManager.CurrentCredential = AccountManager.CreateCredential(username, password);
                        }
                        catch(InvalidOperationException ex)
                        {
                            await loadReCapcha();
                            this.tb_info.Text = ex.GetMessage();
                            this.tb_user.Focus(FocusState.Programmatic);
                            args.Cancel = true;
                        }
                        catch(Exception ex)
                        {
                            this.tb_info.Text = ex.GetMessage();
                            this.tb_user.Focus(FocusState.Programmatic);
                            args.Cancel = true;
                        }
                    }
                    finally
                    {
                        d.Complete();
                    }
                }
            }
            finally
            {
                await Dispatcher.YieldIdle();
                this.pb_Loading.IsIndeterminate = false;
            }
        }

        private async void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            switch(args.Result)
            {
            case ContentDialogResult.None:
            case ContentDialogResult.Primary:
                if(this.pb_Loading.IsIndeterminate)
                {
                    await logOnAsync(args);
                }
                else
                {
                    if(args.Result == ContentDialogResult.None && Client.Current.NeedLogOn)
                    {
                        args.Cancel = true;
                    }
                }
                break;
            case ContentDialogResult.Secondary:
                if(Client.Current.NeedLogOn)
                {
                    Application.Current.Exit();
                }
                break;
            }
        }

        private void tb_TextChanged(object sender, RoutedEventArgs e)
        {
            this.tb_info.Text = "";
        }

        private async void btn_ReloadReCaptcha_Click(object sender, RoutedEventArgs e)
        {
            await loadReCapcha();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if(Client.Current.NeedLogOn)
                this.SecondaryButtonText = Strings.Resources.General.Exit;
            else
                this.SecondaryButtonText = Strings.Resources.General.Cancel;
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            if(e.OriginalKey == Windows.System.VirtualKey.Enter)
            {
                e.Handled = true;
                if(string.IsNullOrWhiteSpace(this.tb_user.Text))
                {
                    this.tb_user.Focus(FocusState.Programmatic);
                }
                else if(string.IsNullOrEmpty(this.pb_pass.Password))
                {
                    this.pb_pass.Focus(FocusState.Programmatic);
                }
                else if(this.sp_ReCaptcha.Visibility == Visibility.Visible && string.IsNullOrEmpty(this.tb_ReCaptcha.Text))
                {
                    this.tb_ReCaptcha.Focus(FocusState.Programmatic);
                }
                else
                {
                    this.pb_Loading.IsIndeterminate = true;
                    this.Hide();
                }
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.pb_Loading.IsIndeterminate = true;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.pb_Loading.IsIndeterminate = false;
        }
    }
}
