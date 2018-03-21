using ExClient;
using ExClient.Api;
using ExClient.Galleries;
using ExClient.Galleries.Metadata;
using ExClient.Galleries.Rating;
using ExViewer.Helpers;
using ExViewer.Services;
using ExViewer.Settings;
using ExViewer.Views;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExViewer.ViewModels
{
    public enum OperationState
    {
        NotStarted,
        Started,
        Failed,
        Completed
    }

    public class GalleryVM : ViewModelBase
    {
        private static AutoFillCacheStorage<GalleryInfo, GalleryVM> Cache { get; }
            = AutoFillCacheStorage.Create(gi => Run(async token => new GalleryVM(await gi.FetchGalleryAsync())), 25, new GalleryInfoComparer());

        private class GalleryInfoComparer : IEqualityComparer<GalleryInfo>
        {
            public bool Equals(GalleryInfo x, GalleryInfo y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(GalleryInfo obj)
            {
                return obj.ID.GetHashCode();
            }
        }

        public static bool RemoveVM(long id) => Cache.Remove(new GalleryInfo(id, default));

        public static GalleryVM GetVM(Gallery gallery)
        {
            var gi = new GalleryInfo(gallery.ID, gallery.Token);
            if (Cache.TryGetValue(gi, out var vm))
            {
                vm.Gallery = gallery;
            }
            else
            {
                vm = new GalleryVM(gallery);
                Cache.Add(gi, vm);
            }
            return vm;
        }

        public static GalleryVM GetVM(long id) => Cache[new GalleryInfo(id, default)];

        public static IAsyncOperation<GalleryVM> GetVMAsync(GalleryInfo gInfo)
        {
            return Cache.GetOrCreateAsync(gInfo);
        }

        private GalleryVM() { }

        private GalleryVM(Gallery gallery)
            : this()
        {
            this.Gallery = gallery;
        }

        protected override IReadOnlyDictionary<string, System.Windows.Input.ICommand> Commands { get; }
            = new Dictionary<string, System.Windows.Input.ICommand>
            {
                [nameof(Rate)] = AsyncCommand.Create<Score>(async (c, s) =>
                {
                    var that = (GalleryVM)c.Tag;
                    var rt = that.gallery?.Rating;
                    if (rt is null || rt.UserScore == s)
                        return;
                    try
                    {
                        await rt.RatingAsync(s);
                        return;
                    }
                    catch
                    {
                        rt.OnPropertyChanged(nameof(RatingStatus.UserScore));
                        throw;
                    }
                }, (c, s) => ((GalleryVM)c.Tag).gallery?.Rating != null),
                [nameof(GoToLatestRevision)] = Command.Create<RevisionCollection>(async (sender, c) =>
                {
                    var info = c.DescendantsInfo.Last().Gallery;
                    var load = GetVMAsync(info);
                    if (load.Status != AsyncStatus.Completed)
                        RootControl.RootController.TrackAsyncAction(load, async (s, e) =>
                        {
                            await DispatcherHelper.YieldIdle();
                            await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), info.ID);
                        });
                    else
                        await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), info.ID);
                }, (sender, c) => c != null && c.DescendantsInfo.Count != 0),
                [nameof(Share)] = Command.Create<GalleryImage>(async (sender, image) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    if (!ShareHandler.IsShareSupported)
                    {
                        if (image == null)
                            await Launcher.LaunchUriAsync(that.gallery.GalleryUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                        else
                            await Launcher.LaunchUriAsync(image.PageUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                        return;
                    }
                    var gallery = that.gallery;
                    ShareHandler.Share(async (s, e) =>
                    {
                        var deferral = e.Request.GetDeferral();
                        try
                        {
                            var data = e.Request.Data;
                            data.Properties.Title = gallery.GetDisplayTitle();
                            data.Properties.Description = gallery.GetSecondaryTitle();
                            if (image == null)
                            {
                                data.Properties.ContentSourceWebLink = gallery.GalleryUri;
                                data.SetWebLink(gallery.GalleryUri);
                                data.SetText(gallery.GalleryUri.ToString());
                                if (gallery.Thumb != null)
                                {
                                    var ms = new InMemoryRandomAccessStream();
                                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms);
                                    encoder.SetSoftwareBitmap(gallery.Thumb);
                                    await encoder.FlushAsync();
                                    data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(ms);
                                    var firstImage = gallery.FirstOrDefault()?.ImageFile;
                                    if (firstImage != null)
                                        data.SetBitmap(RandomAccessStreamReference.CreateFromFile(firstImage));
                                    else
                                        data.SetBitmap(RandomAccessStreamReference.CreateFromStream(ms));
                                }
                                var imageFiles = gallery
                                    .Where(i => i.ImageFile != null)
                                    .Select(i => new { i.ImageFile, Name = $"{i.PageID}{i.ImageFile.FileType}" })
                                    .Where(f => f.ImageFile != null)
                                    .ToList();
                                if (imageFiles.Count == 0)
                                    return;
                                data.SetFolderProvider(imageFiles.Select(f => f.ImageFile), imageFiles.Select(f => f.Name), gallery.GetDisplayTitle());
                            }
                            else
                            {
                                data.Properties.ContentSourceWebLink = image.PageUri;
                                data.SetWebLink(image.PageUri);
                                data.SetText(image.PageUri.ToString());
                                var imageFile = image.ImageFile;
                                if (imageFile == null)
                                    return;
                                var view = RandomAccessStreamReference.CreateFromFile(imageFile);
                                data.SetBitmap(view);
                                data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(await imageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem));
                                var fileName = $"{image.PageID}{imageFile.FileType}";
                                data.SetFileProvider(imageFile, fileName);
                            }
                        }
                        finally
                        {
                            deferral.Complete();
                        }
                    });
                }, (sender, image) => ((GalleryVM)sender.Tag).gallery != null),
                [nameof(Save)] = Command.Create(sender =>
                {
                    var that = (GalleryVM)sender.Tag;
                    that.SaveStatus = OperationState.Started;
                    that.SaveProgress = -1;
                    var task = that.gallery.SaveAsync(SettingCollection.Current.GetStrategy());
                    task.Progress = (s, e) =>
                    {
                        that.SaveProgress = e.ImageLoaded * 100.0 / e.ImageCount;
                    };
                    task.Completed = (s, e) =>
                    {
                        switch (e)
                        {
                        case AsyncStatus.Canceled:
                        case AsyncStatus.Error:
                            that.SaveStatus = OperationState.Failed;
                            RootControl.RootController.SendToast(s.ErrorCode, null);
                            break;
                        case AsyncStatus.Completed:
                            that.SaveStatus = OperationState.Completed;
                            break;
                        case AsyncStatus.Started:
                            that.SaveStatus = OperationState.Started;
                            break;
                        }
                        that.SaveProgress = 100;
                    };
                }, sender =>
                {
                    var that = (GalleryVM)sender.Tag;
                    if (that.SaveStatus == OperationState.Started)
                        return false;
                    if (that.gallery is SavedGallery)
                        return false;
                    return true;
                }),
                [nameof(OpenImage)] = Command.Create<GalleryImage>(async (sender, image) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    that.View.MoveCurrentToPosition(image.PageID - 1);
                    await RootControl.RootController.Navigator.NavigateAsync(typeof(ImagePage), that.gallery.ID);
                }, (sender, image) => image != null),
                [nameof(LoadOriginal)] = Command.Create<GalleryImage>(async (sender, image) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    try
                    {
                        await image.LoadImageAsync(true, ConnectionStrategy.AllFull, true);
                    }
                    catch (Exception ex)
                    {
                        RootControl.RootController.SendToast(ex, typeof(ImagePage));
                    }
                }, (sender, image) => image != null && !image.OriginalLoaded),
                [nameof(ReloadImage)] = Command.Create<GalleryImage>(async (sender, image) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    try
                    {
                        if (image.OriginalLoaded)
                            await image.LoadImageAsync(true, ConnectionStrategy.AllFull, true);
                        else
                            await image.LoadImageAsync(true, SettingCollection.Current.GetStrategy(), true);
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        RootControl.RootController.SendToast(ex, typeof(ImagePage));
                    }
                }, (sender, image) => image != null),
                [nameof(SearchImage)] = Command.Create<SHA1Value>(async (sender, hash) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    var search = Client.Current.Search("", Category.All, Enumerable.Repeat(hash, 1), that.gallery.GetDisplayTitle());
                    var vm = SearchVM.GetVM(search);
                    await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
                }, (sender, hash) => ((GalleryVM)sender.Tag).gallery != null && hash != default),
                [nameof(SearchUploader)] = Command.Create(async sender =>
                {
                    var that = (GalleryVM)sender.Tag;
                    var search = Client.Current.Search(that.gallery.Uploader, null, SettingCollection.Current.DefaultSearchCategory);
                    var vm = SearchVM.GetVM(search);
                    await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
                }, sender => ((GalleryVM)sender.Tag).gallery != null),
                [nameof(TorrentDownload)] = Command.Create<TorrentInfo>(async (sender, torrent) =>
                {
                    RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TorrentDownloading, null);
                    try
                    {
                        var file = await torrent.LoadTorrentAsync();
                        if (torrentfolder == null)
                            await loadTorrentFolder();
                        await file.MoveAsync(torrentfolder, file.Name, NameCollisionOption.GenerateUniqueName);
                        if (!await Launcher.LaunchFileAsync(file))
                            await Launcher.LaunchFolderAsync(torrentfolder);
                        RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TorrentDownloaded(torrentfolder.Path), null);
                    }
                    catch (Exception ex)
                    {
                        RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                    }
                }, (sender, torrent) => torrent != null && torrent.TorrentUri != null),
                [nameof(AddComment)] = AsyncCommand.Create(async (sender) =>
                {
                    var that = (GalleryVM)sender.Tag;
                    var addComment = System.Threading.LazyInitializer.EnsureInitialized(ref GalleryVM.addCommentDialog);
                    addComment.Gallery = that.Gallery;
                    await addComment.ShowAsync();
                }, sender => ((GalleryVM)sender.Tag).Gallery != null),
            };

        private static AddCommentDialog addCommentDialog;

        public AsyncCommand<Score> Rate => GetCommand<AsyncCommand<Score>>();

        public Command<RevisionCollection> GoToLatestRevision => GetCommand<Command<RevisionCollection>>();

        public Command<GalleryImage> Share => GetCommand<Command<GalleryImage>>();

        public Command Save => GetCommand<Command>();

        public Command<GalleryImage> OpenImage => GetCommand<Command<GalleryImage>>();

        public Command<GalleryImage> LoadOriginal => GetCommand<Command<GalleryImage>>();

        public Command<GalleryImage> ReloadImage => GetCommand<Command<GalleryImage>>();

        public Command<SHA1Value> SearchImage => GetCommand<Command<SHA1Value>>();

        public Command SearchUploader => GetCommand<Command>();

        private Gallery gallery;

        public Gallery Gallery
        {
            get => this.gallery;
            private set
            {
                if (View != null)
                {
                    View.CurrentChanged -= this.View_CurrentChanged;
                }
                View = value?.CreateView();
                if (View != null)
                {
                    View.CurrentChanged += this.View_CurrentChanged;
                }
                Set(nameof(View), ref this.gallery, value);
                this.Save.OnCanExecuteChanged();
                this.Share.OnCanExecuteChanged();
                this.AddComment.OnCanExecuteChanged();
                this.Torrents = null;
            }
        }

        private void View_CurrentChanged(object sender, object e)
        {
            CurrentInfo = null;
            QRCodeResult = null;
        }

        public ICollectionView View { get; private set; }

        private string currentInfo;
        public string CurrentInfo { get => this.currentInfo; private set => Set(ref this.currentInfo, value); }

        private string qrCodeResult;
        private int qrCodeHash;
        public string QRCodeResult
        {
            get => this.qrCodeResult;
            private set
            {
                Set(ref this.qrCodeResult, value);
                this.qrCodeHash = 0;
            }
        }

        public IAsyncAction RefreshInfoAsync()
        {
            var current = (GalleryImage)this.View.CurrentItem;
            var imageFile = current?.ImageFile;
            if (imageFile == null)
            {
                this.CurrentInfo = Strings.Resources.Views.ImagePage.ImageFileInfoDefault;
                this.QRCodeResult = null;
                return AsyncAction.CreateCompleted();
            }
            return Task.Run(async () =>
            {
                var prop = await imageFile.GetBasicPropertiesAsync();
                var imageProp = await imageFile.Properties.GetImagePropertiesAsync();
                this.CurrentInfo = Strings.Resources.Views.ImagePage.ImageFileInfo(
                    fileType: imageFile.DisplayType,
                    size: Opportunity.Converters.Typed.ByteSizeToStringConverter.ByteSizeToString((long)prop.Size, Opportunity.Converters.Typed.UnitPrefix.Binary),
                    width: imageProp.Width, height: imageProp.Height);
                var newQRHash = prop.Size.GetHashCode() ^ imageFile.Name.GetHashCode();
                if (newQRHash == this.qrCodeHash)
                    return;
                using (var stream = await imageFile.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var sb = await decoder.GetSoftwareBitmapAsync();
                    var r = new ZXing.QrCode.QRCodeReader();
                    var qr = r.decode(new ZXing.BinaryBitmap(new ZXing.Common.HybridBinarizer(new ZXing.SoftwareBitmapLuminanceSource(sb))));
                    this.QRCodeResult = qr?.Text;
                    this.qrCodeHash = newQRHash;
                }
            }).AsAsyncAction();
        }

        private static ContentDialogNotificationData qrCodeDialog = new ContentDialogNotificationData
        {
            Title = Strings.Resources.Views.QRCodeDialog.Title,
            PrimaryButtonText = Strings.Resources.Views.QRCodeDialog.PrimaryButtonText,
            PrimaryButtonCommand = Opportunity.MvvmUniverse.Commands.Predefined.DataTransferCommands.SetClipboard,
            CloseButtonText = Strings.Resources.Views.QRCodeDialog.CloseButtonText,
        };
        public Command<string> OpenQRCode { get; } = Command.Create<string>(async (sender, code) =>
        {
            var opened = false;
            try
            {
                var uri = new Uri(code);
                opened = await Launcher.LaunchUriAsync(uri);
            }
            catch (Exception)
            {
            }
            if (!opened)
            {
                if (!(qrCodeDialog.Content is TextBlock tb))
                {
                    tb = new TextBlock { IsTextSelectionEnabled = true, TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap };
                    qrCodeDialog.Content = tb;
                }
                tb.Text = code;
                qrCodeDialog.PrimaryButtonCommandParameter = code;
                await Notificator.GetForCurrentView().NotifyAsync(qrCodeDialog);
            }
        }, (sender, code) => code != null);

        private OperationState saveStatus;

        public OperationState SaveStatus
        {
            get => this.saveStatus;
            set
            {
                Set(ref this.saveStatus, value);
                this.Save.OnCanExecuteChanged();
            }
        }

        private double saveProgress;

        public double SaveProgress
        {
            get => this.saveProgress;
            set => Set(ref this.saveProgress, value);
        }

        #region Comments

        public AsyncCommand AddComment => GetCommand<AsyncCommand>();

        public IAsyncAction LoadComments()
        {
            return Run(async token =>
            {
                try
                {
                    await this.gallery.Comments.FetchAsync();
                }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            });
        }

        #endregion Comments

        #region Torrents

        private static StorageFolder torrentfolder;

        private static IAsyncAction loadTorrentFolder()
        {
            return Run(async token =>
            {
                var ftoken = StatusCollection.Current.TorrentFolderToken;
                if (ftoken != null)
                {
                    try
                    {
                        torrentfolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(ftoken);
                    }
                    catch (Exception)
                    {
                        //Load failed
                        torrentfolder = null;
                    }
                }
                if (torrentfolder == null)
                {
                    torrentfolder = await DownloadsFolder.CreateFolderAsync("Torrents", CreationCollisionOption.GenerateUniqueName);
                    if (ftoken == null)
                        ftoken = StorageApplicationPermissions.FutureAccessList.Add(torrentfolder);
                    else
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace(ftoken, torrentfolder);
                }
                StatusCollection.Current.TorrentFolderToken = ftoken;
            });
        }

        public Command<TorrentInfo> TorrentDownload => GetCommand<Command<TorrentInfo>>();

        public IAsyncAction LoadTorrents()
        {
            return Run(async token =>
            {
                try
                {
                    this.Torrents = await this.gallery.FetchTorrnetsAsync();
                }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            });
        }

        private ReadOnlyCollection<TorrentInfo> torrents;

        public ReadOnlyCollection<TorrentInfo> Torrents
        {
            get => this.torrents;
            private set
            {
                this.torrents = value;
                OnPropertyChanged(nameof(Torrents), nameof(TorrentCount));
            }
        }

        public int? TorrentCount => this.torrents?.Count ?? (this.gallery is CachedGallery ? null : this.gallery?.TorrentCount);

        #endregion Torrents
    }
}
