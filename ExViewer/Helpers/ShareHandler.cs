using ExViewer.Views;
using Opportunity.MvvmUniverse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            {
                return;
            }

            new ShareHandlerStorage(handler);
            if (ExApiInfo.RS3)
            {
                DataTransferManager.ShowShareUI(new ShareUIOptions { Theme = Settings.SettingCollection.Current.Theme == Windows.UI.Xaml.ApplicationTheme.Dark ? ShareUITheme.Dark : ShareUITheme.Light });
            }
            else
            {
                DataTransferManager.ShowShareUI();
            }
        }

        private static void PrepareFileShare(DataPackage package, List<IStorageFile> files)
        {
            package.RequestedOperation = DataPackageOperation.Move;
            foreach (var item in files.Select(f => f.FileType).Distinct())
            {
                if (item != null)
                {
                    package.Properties.FileTypes.Add(item);
                }
            }
        }

        public static void SetFileProvider(this DataPackage package, IStorageFile file, string fileName)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            fileName = StorageHelper.ToValidFileName(fileName);
            var fileList = new List<IStorageFile> { file };
            PrepareFileShare(package, fileList);
            package.SetDataProvider(StandardDataFormats.StorageItems, async req =>
            {
                var def = req.GetDeferral();
                try
                {
                    var tempFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("DataRequested", CreationCollisionOption.OpenIfExists);
                    var tempFile = await file.CopyAsync(tempFolder, fileName, NameCollisionOption.ReplaceExisting);
                    req.SetData(new StorageItemContainer(tempFile));
                }
                finally
                {
                    def.Complete();
                }
            });
        }

        public static void SetFolderProvider(this DataPackage package, IEnumerable<IStorageFile> files, IEnumerable<string> fileNames, string folderName)
        {
            folderName = StorageHelper.ToValidFileName(folderName);
            var fileList = files.ToList();
            var nameList = fileNames.Select(StorageHelper.ToValidFileName).ToList();
            if (fileList.Count != nameList.Count)
            {
                throw new ArgumentException("files count != fileNames count");
            }

            PrepareFileShare(package, fileList);
            package.SetDataProvider(StandardDataFormats.StorageItems, async req =>
            {
                var def = req.GetDeferral();
                try
                {
                    var tempFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("DataRequested", CreationCollisionOption.OpenIfExists);
                    var dataFolder = await tempFolder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
                    var targetList = new List<IStorageFile>();
                    for (var i = 0; i < fileList.Count; i++)
                    {
                        targetList.Add(await fileList[i].CopyAsync(dataFolder, nameList[i], NameCollisionOption.GenerateUniqueName));
                    }
                    req.SetData(new StorageItemContainer(dataFolder));
                }
                finally
                {
                    def.Complete();
                }
            });
        }

        private class StorageItemContainer : IEnumerable<IStorageItem>
        {
            private readonly IStorageItem item;

            public StorageItemContainer(IStorageItem item)
            {
                this.item = item;
            }

            public IEnumerator<IStorageItem> GetEnumerator()
            {
                yield return this.item;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class ShareHandlerStorage
        {
            public ShareHandlerStorage(TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler)
            {
                this.handler = handler;
                var t = DataTransferManager.GetForCurrentView();
                t.DataRequested += this.T_DataRequested;
                if (CustomHandlers.Instance != null)
                {
                    t.ShareProvidersRequested += this.T_ShareProvidersRequested;
                }
            }

            private void T_ShareProvidersRequested(DataTransferManager sender, ShareProvidersRequestedEventArgs args)
            {
                sender.ShareProvidersRequested -= this.T_ShareProvidersRequested;

                args.Providers.Add(CustomHandlers.Instance.CopyProvider);

                if (args.Data.Contains(StandardDataFormats.WebLink))
                {
                    args.Providers.Add(CustomHandlers.Instance.OpenLinkProvider);
                }

                if (args.Data.Contains(StandardDataFormats.StorageItems))
                {
                    args.Providers.Add(CustomHandlers.Instance.SetWallpaperProvider);
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

        private class CustomHandlers
        {
            public static CustomHandlers Instance { get; } = create();

            private static CustomHandlers create()
            {
                if (!ExApiInfo.ShareProviderSupported)
                {
                    return null;
                }

                return new CustomHandlers();
            }
            public ShareProvider OpenLinkProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.OpenInBrowser
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Assets/ShareTarget/MicrosoftEdge.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var uri = await operation.Data.GetWebLinkAsync();
                        await CoreApplication.MainView.Dispatcher.YieldIdle();
                        await Launcher.LaunchUriAsync(uri, new LauncherOptions { IgnoreAppUriHandlers = true });
                        operation.ReportCompleted();
                    });
            public ShareProvider CopyProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.CopyToClipboard
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Assets/ShareTarget/CopyToClipboard.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var data = operation.Data;
                        var pac = await DataProviderProxy.CreateAsync(data);
                        await CoreApplication.MainView.Dispatcher.YieldIdle();
                        Clipboard.SetContent(pac.View);
                        RootControl.RootController.SendToast(Strings.Resources.Sharing.CopyedToClipboard, null);
                        operation.ReportCompleted();
                    });
            public ShareProvider SetWallpaperProvider { get; }
                = new ShareProvider(Strings.Resources.Sharing.SetWallpaper
                    , RandomAccessStreamReference.CreateFromUri(new Uri(@"ms-appx:///Assets/ShareTarget/Settings.png"))
                    , (Color)Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]
                    , async operation =>
                    {
                        var files = (await operation.Data.GetStorageItemsAsync()).FirstOrDefault();
                        var file = files as StorageFile;
                        if (file is null)
                        {
                            var folder = files as StorageFolder;
                            if (folder is null)
                            {
                                return;
                            }
                            file = (await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery, 0, 1)).FirstOrDefault();
                        }
                        if (file is null)
                        {
                            return;
                        }
                        // Only files in localfolder can be set as background.
                        file = await file.CopyAsync(ApplicationData.Current.LocalFolder, $"Img_{file.Name}", NameCollisionOption.ReplaceExisting);
                        await CoreApplication.MainView.Dispatcher.YieldIdle();
                        var succeeded = await User​Profile​Personalization​Settings.Current.TrySetWallpaperImageAsync(file);
                        if (succeeded)
                        {
                            RootControl.RootController.SendToast(Strings.Resources.Sharing.SetWallpaperSucceeded, null);
                        }
                        else
                        {
                            RootControl.RootController.SendToast(Strings.Resources.Sharing.SetWallpaperFailed, null);
                        }

                        await Task.Delay(10000).ConfigureAwait(false);
                        await file.DeleteAsync();
                        operation.ReportCompleted();
                    });

            private class DataProviderProxy
            {
                public static IAsyncOperation<DataProviderProxy> CreateAsync(DataPackageView viewToProxy)
                {
                    if (viewToProxy is null)
                    {
                        throw new ArgumentNullException(nameof(viewToProxy));
                    }

                    var proxy = new DataProviderProxy(viewToProxy);
                    return proxy.initAsync();
                }

                private DataProviderProxy(DataPackageView viewToProxy)
                {
                    this.viewToProxy = viewToProxy;
                    this.View = new DataPackage();
                }

                private IAsyncOperation<DataProviderProxy> initAsync()
                {
                    return AsyncInfo.Run(async token =>
                    {
                        var view = this.View;
                        view.RequestedOperation = this.viewToProxy.RequestedOperation;
                        foreach (var item in await this.viewToProxy.GetResourceMapAsync())
                        {
                            view.ResourceMap.Add(item.Key, item.Value);
                        }
                        foreach (var item in this.viewToProxy.Properties)
                        {
                            view.Properties.Add(item.Key, item.Value);
                        }
                        foreach (var formatId in this.viewToProxy.AvailableFormats)
                        {
                            if (!view.Properties.FileTypes.Contains(formatId))
                            {
                                view.Properties.FileTypes.Add(formatId);
                            }

                            view.SetDataProvider(formatId, this.dataProvider);
                        }
                        return this;
                    });
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

                private DataPackageView viewToProxy;

                public DataPackage View { get; }
            }
        }
    }
}
