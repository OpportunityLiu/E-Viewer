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
            var username = tb_user.Text;
            if(string.Equals("eviewer", username))
                Settings.SettingCollection.Current.DefaultSearchCategory = Category.NonH;
            var password = pb_pass.Password;
            if(string.IsNullOrWhiteSpace(username))
            {
                tb_info.Text = LocalizedStrings.Resources.LogOnDialogNoUserName;
                tb_user.Focus(FocusState.Programmatic);
                args.Cancel = true;
            }
            else if(string.IsNullOrEmpty(password))
            {
                tb_info.Text = LocalizedStrings.Resources.LogOnDialogNoPassword;
                pb_pass.Focus(FocusState.Programmatic);
                args.Cancel = true;
            }
            else
            {
                pb_Loading.IsIndeterminate = true;
                var d = args.GetDeferral();
                tb_info.Text = "";
                try
                {
                    if(recap != null)
                        await recap.Submit(tb_ReCaptcha.Text);
                }
                catch(Exception ex)
                {
                    await loadReCapcha();
                    tb_info.Text = ex.GetMessage();
                    tb_ReCaptcha.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                try
                {
                    if(args.Cancel)
                        return;
                    await Client.Current.LogOnAsync(username, password, recap);
                    AccountManager.CurrentCredential = AccountManager.CreateCredential(username, password);
                }
                catch(ArgumentException ex) when(ex.ParamName == "response")
                {
                }
                catch(NotSupportedException ex)
                {
                    tb_user.Text = "";
                    pb_pass.Password = "";
                    await Task.Delay(50);
                    tb_info.Text = ex.GetMessage();
                    tb_user.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                catch(InvalidOperationException ex)
                {
                    await loadReCapcha();
                    tb_info.Text = ex.GetMessage();
                    tb_user.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                catch(Exception ex)
                {
                    tb_info.Text = ex.GetMessage();
                    tb_user.Focus(FocusState.Programmatic);
                    args.Cancel = true;
                }
                finally
                {
                    pb_Loading.IsIndeterminate = false;
                    d.Complete();
                }
            }
        }

        private async Task loadReCapcha()
        {
            sp_ReCaptcha.Visibility = Visibility.Visible;
            img_ReCaptcha.Source = null;
            tb_ReCaptcha.Text = "";
            try
            {
                recap = await ReCaptcha.Fetch();
                img_ReCaptcha.Source = new BitmapImage(recap.ImageUri);
            }
            catch(Exception ex)
            {
                tb_info.Text = ex.GetMessage();
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
            tb_info.Text = "";
        }

        private async void btn_ReloadReCaptcha_Click(object sender, RoutedEventArgs e)
        {
            await loadReCapcha();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if(Client.Current.NeedLogOn)
                this.SecondaryButtonText = LocalizedStrings.Resources.Exit;
            else
                this.SecondaryButtonText = LocalizedStrings.Resources.Cancel;
        }
    }
}
