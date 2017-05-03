using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

namespace ExViewer.Helpers
{
    public static class ShareHandler
    {
        public static bool IsShareSupported => DataTransferManager.IsSupported();

        public static void Share(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
        {
            new ShareHandlerStorage(handler);
            DataTransferManager.ShowShareUI();
        }


        private class ShareHandlerStorage
        {
            private static ShareProvider openLinkProvider
                = new ShareProvider(Strings.Resources.Sharing.OpenInBrowser
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Images/MicrosoftEdgeSquare44x44.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var uri = await operation.Data.GetWebLinkAsync();
                        await Task.Delay(100);
                        DispatcherHelper.BeginInvokeOnUIThread(async () =>
                        {
                            await Launcher.LaunchUriAsync(uri, new LauncherOptions { IgnoreAppUriHandlers = true });
                        });
                    });

            public ShareHandlerStorage(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
            {
                this.handler = handler;
                var t = DataTransferManager.GetForCurrentView();
                t.DataRequested += this.T_DataRequested;
                t.ShareProvidersRequested += this.T_ShareProvidersRequested;
            }

            private void T_ShareProvidersRequested(DataTransferManager sender, ShareProvidersRequestedEventArgs args)
            {
                sender.ShareProvidersRequested -= this.T_ShareProvidersRequested;
                if (args.Data.Contains(StandardDataFormats.WebLink))
                {
                    args.Providers.Add(openLinkProvider);
                }
            }

            private TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler;

            private void T_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
            {
                sender.DataRequested -= this.T_DataRequested;
                var d = args.Request.Data;
                d.Properties.Title = Package.Current.DisplayName;
                d.Properties.ApplicationName = Package.Current.DisplayName;
                d.Properties.PackageFamilyName = Package.Current.Id.FamilyName;
                this.handler?.Invoke(sender, args);
            }
        }
    }
}
