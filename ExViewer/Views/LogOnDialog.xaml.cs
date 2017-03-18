using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Credentials;

// “内容对话框”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上进行了说明

namespace ExViewer.Views
{
    public sealed partial class LogOnDialog : ContentDialog
    {
        public LogOnDialog()
        {
            this.InitializeComponent();
            this.RequestedTheme = Settings.SettingCollection.Current.Theme.ToElementTheme();
        }

        ReCaptcha recap;

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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
                    this.pb_Loading.IsIndeterminate = true;
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
                    this.pb_Loading.IsIndeterminate = false;
                    d.Complete();
                }
            }
        }

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

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(Client.Current.NeedLogOn)
            {
                Application.Current.Exit();
            }
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if(args.Result == ContentDialogResult.None && Client.Current.NeedLogOn)
            {
                args.Cancel = true;
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
                this.SecondaryButtonText = Strings.Resources.Exit;
            else
                this.SecondaryButtonText = Strings.Resources.Cancel;
        }
    }
}
