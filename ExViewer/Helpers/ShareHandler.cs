using ExViewer.Views;
using Opportunity.MvvmUniverse.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.Core;

namespace ExViewer.Helpers
{
    public static class ShareHandler
    {
        public static bool IsShareSupported => DataTransferManager.IsSupported();

        public static void Share(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
        {
            if (!IsShareSupported)
                return;
            new ShareHandlerStorage(handler);
            DataTransferManager.ShowShareUI();
        }

        private class ShareHandlerStorage
        {
            public ShareHandlerStorage(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
            {
                this.handler = handler;
                var t = DataTransferManager.GetForCurrentView();
                t.DataRequested += this.T_DataRequested;
                if (CustomHandlers.Instance != null)
                    t.ShareProvidersRequested += this.T_ShareProvidersRequested;
            }

            private void T_ShareProvidersRequested(DataTransferManager sender, ShareProvidersRequestedEventArgs args)
            {
                sender.ShareProvidersRequested -= this.T_ShareProvidersRequested;

                args.Providers.Add(CustomHandlers.Instance.CopyProvider);

                if (args.Data.Contains(StandardDataFormats.WebLink))
                    args.Providers.Add(CustomHandlers.Instance.OpenLinkProvider);

                if (args.Data.Contains(StandardDataFormats.StorageItems))
                    args.Providers.Add(CustomHandlers.Instance.SetWallpaperProvider);
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

        private class CustomHandlers
        {
            public static CustomHandlers Instance { get; } = create();

            private static CustomHandlers create()
            {
                if (!ApiInfo.ShareProviderSupported)
                    return null;
                return new CustomHandlers();
            }
            public ShareProvider OpenLinkProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.OpenInBrowser
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Images/MicrosoftEdge.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var uri = await operation.Data.GetWebLinkAsync();
                        await Task.Delay(100);
                        DispatcherHelper.BeginInvokeOnUIThread(async () =>
                        {
                            await Launcher.LaunchUriAsync(uri, new LauncherOptions { IgnoreAppUriHandlers = true });
                            operation.ReportCompleted();
                        });
                    });
            public ShareProvider CopyProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.CopyToClipboard
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Images/CopyToClipboard.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var data = operation.Data;
                        var pac = await DataProviderProxy.CreateAsync(data);
                        await Task.Delay(500);
                        DispatcherHelper.BeginInvokeOnUIThread(() =>
                        {
                            Clipboard.SetContent(pac.View);
                            RootControl.RootController.SendToast(Strings.Resources.Sharing.CopyedToClipboard, null);
                            operation.ReportCompleted();
                        });
                    });
            public ShareProvider SetWallpaperProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.SetWallpaper
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Images/Settings.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var files = (await operation.Data.GetStorageItemsAsync()).FirstOrDefault();
                        var file = files as StorageFile;
                        if (file == null)
                        {
                            var folder = files as StorageFolder;
                            if (folder == null)
                            {
                                return;
                            }
                            file = (await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery, 0, 1)).FirstOrDefault();
                        }
                        if (file == null)
                            return;
                        file = await file.CopyAsync(ApplicationData.Current.LocalFolder, $"{file.Path.GetHashCode()}{file.FileType}", NameCollisionOption.GenerateUniqueName);
                        await Task.Delay(500);
                        DispatcherHelper.BeginInvokeOnUIThread(async () =>
                        {
                            var succeeded = await User​Profile​Personalization​Settings.Current.TrySetWallpaperImageAsync(file);
                            if (succeeded)
                                RootControl.RootController.SendToast(Strings.Resources.Sharing.SetWallpaperSucceeded, null);
                            else
                                RootControl.RootController.SendToast(Strings.Resources.Sharing.SetWallpaperFailed, null);
                            await Task.Delay(10000);
                            await file.DeleteAsync();
                            operation.ReportCompleted();
                        });
                    });

            private class DataProviderProxy
            {
                private DataPackageView viewToProxy;

                public static IAsyncOperation<DataProviderProxy> CreateAsync(DataPackageView viewToProxy)
                {
                    if (viewToProxy == null)
                        throw new ArgumentNullException(nameof(viewToProxy));
                    return AsyncInfo.Run(async token =>
                    {
                        var proxy = new DataProviderProxy(viewToProxy);
                        var view = proxy.View;
                        foreach (var item in await viewToProxy.GetResourceMapAsync())
                        {
                            view.ResourceMap.Add(item.Key, item.Value);
                        }
                        foreach (var item in viewToProxy.Properties)
                        {
                            view.Properties.Add(item.Key, item.Value);
                        }
                        foreach (var formatId in viewToProxy.AvailableFormats)
                        {
                            if (!view.Properties.FileTypes.Contains(formatId))
                                view.Properties.FileTypes.Add(formatId);
                            view.SetDataProvider(formatId, proxy.dataProvider);
                        }
                        return proxy;
                    });
                }

                private DataProviderProxy(DataPackageView viewToProxy)
                {
                    this.viewToProxy = viewToProxy;
                    this.View = new DataPackage
                    {
                        RequestedOperation = viewToProxy.RequestedOperation
                    };
                }

                private async void dataProvider(DataProviderRequest request)
                {
                    var d = request.GetDeferral();
                    try
                    {
                        var result = await this.viewToProxy.GetDataAsync(request.FormatId);
                        request.SetData(result);
                    }
                    finally
                    {
                        d.Complete();
                    }
                }

                public DataPackage View
                {
                    get;
                }
            }
        }
    }
}
