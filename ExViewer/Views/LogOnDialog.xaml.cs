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
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var d = args.GetDeferral();
            await Client.CreateClient(tb_user.Text, tb_pass.Password);
            if(cb_savepass.IsChecked == true)
            {
                var pv = new PasswordVault();
                try
                {
                    var oldpass = pv.FindAllByResource("ex").First();
                    pv.Remove(oldpass);
                }
                catch(Exception ex) when(ex.HResult == -2147023728)
                {
                }
                pv.Add(new PasswordCredential("ex", tb_user.Text, tb_pass.Password));
            }
            d.Complete();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
