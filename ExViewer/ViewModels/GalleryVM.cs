using ExClient;
using ExClient.Api;
using ExClient.Galleries;
using ExClient.Galleries.Metadata;
using ExClient.Galleries.Rating;
using ExClient.Services;
using ExViewer.Database;
using ExViewer.Helpers;
using ExViewer.Services;
using ExViewer.Settings;
using ExViewer.Views;
using Opportunity.Helpers.ObjectModel;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Commands.ReentrancyHandlers;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
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
            var gi = new GalleryInfo(gallery.Id, gallery.Token);
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

        private GalleryVM(Gallery gallery)
        {
            Gallery = gallery;
        }

        public AsyncCommand<Score> Rate => Commands.GetOrAdd(() =>
            AsyncCommand<Score>.Create(async (c, s) =>
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
            }, (c, s) => ((GalleryVM)c.Tag).gallery?.Rating != null && s != Score.NotSet && ((GalleryVM)c.Tag).gallery.Rating.UserScore != s));

        public AsyncCommand AddToFavorites => Commands.GetOrAdd(() =>
        {
            var ac = AsyncCommand.Create(async (c, s) =>
            {
                var addToFavorites = ThreadLocalSingleton.GetOrCreate<AddToFavoritesDialog>();
                addToFavorites.Gallery = Gallery;
                await addToFavorites.ShowAsync();
            });
            ac.ReentrancyHandler = ReentrancyHandler.LastQueued();
            return ac;
        });

        public AsyncCommand Rename => Commands.GetOrAdd(() =>
            AsyncCommand.Create(async (c, s) =>
            {
                var rename = ThreadLocalSingleton.GetOrCreate<RenameGalleryDialog>();
                rename.Gallery = Gallery;
                await rename.ShowAsync();
            }));

        public AsyncCommand Expunge => Commands.GetOrAdd(() =>
            AsyncCommand.Create(async (c, s) =>
            {
                var expunge = ThreadLocalSingleton.GetOrCreate<ExpungeGalleryDialog>();
                expunge.Gallery = Gallery;
                await expunge.ShowAsync();
            }));

        public Command<RevisionCollection> GoToLatestRevision => Commands.GetOrAdd(() =>
            Command<RevisionCollection>.Create(async (sender, c) =>
            {
                var info = c.DescendantsInfo.Last().Gallery;
                var load = GetVMAsync(info);
                if (load.Status != AsyncStatus.Completed)
                {
                    RootControl.RootController.TrackAsyncAction(load, async (s, e) =>
                    {
                        await RootControl.RootController.Frame.Dispatcher.YieldIdle();
                        await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), info.ID);
                    });
                }
                else
                {
                    await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), info.ID);
                }
            }, (sender, c) => (c?.DescendantsInfo?.Count).GetValueOrDefault() != 0));

        public Command<GalleryImage> Share => Commands.GetOrAdd(() =>
            Command<GalleryImage>.Create(async (sender, image) =>
            {
                var that = (GalleryVM)sender.Tag;
                if (!ShareHandler.IsShareSupported)
                {
                    if (image is null)
                    {
                        await Launcher.LaunchUriAsync(that.gallery.GalleryUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                    }
                    else
                    {
                        await Launcher.LaunchUriAsync(image.PageUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                    }

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
                        if (image is null)
                        {
                            data.Properties.ContentSourceWebLink = gallery.GalleryUri;
                            data.SetWebLink(gallery.GalleryUri);
                            data.SetText(gallery.GalleryUri.ToString());
                            var firstImage = gallery.FirstOrDefault()?.ImageFile;
                            if (firstImage != null)
                                data.SetBitmap(RandomAccessStreamReference.CreateFromFile(firstImage));
                            try
                            {
                                using (var client = new HttpClient())
                                {
                                    var buf = await client.GetBufferAsync(gallery.ThumbUri);
                                    var ms = new InMemoryRandomAccessStream();
                                    await ms.WriteAsync(buf);
                                    data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(ms);
                                    if (firstImage is null)
                                        data.SetBitmap(RandomAccessStreamReference.CreateFromStream(ms));
                                }
                            }
                            catch { }
                            var imageFiles = gallery
                                .Where(i => i.ImageFile != null)
                                .Select(i => new { i.ImageFile, Name = $"{i.PageId}{i.ImageFile.FileType}" })
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
                            if (imageFile is null)
                            {
                                return;
                            }

                            var view = RandomAccessStreamReference.CreateFromFile(imageFile);
                            data.SetBitmap(view);
                            data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(await imageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem));
                            var fileName = $"{image.PageId}{imageFile.FileType}";
                            data.SetFileProvider(imageFile, fileName);
                        }
                    }
                    finally
                    {
                        deferral.Complete();
                    }
                });
            }, (sender, image) => ((GalleryVM)sender.Tag).gallery != null));

        public Command Save => Commands.GetOrAdd(() =>
            Command.Create(sender =>
            {
                var that = (GalleryVM)sender.Tag;
                that.SaveStatus = OperationState.Started;
                that.SaveProgress = -1;
                var task = that.gallery.SaveAsync(SettingCollection.Current.GetStrategy());
                task.Progress = (s, e) => that.SaveProgress = e.ImageLoaded * 100.0 / e.ImageCount;
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
            }, sender => (sender.Tag is GalleryVM that) && that.SaveStatus != OperationState.Started && !(that.gallery is SavedGallery)));

        public Command<GalleryImage> OpenImage => Commands.GetOrAdd(() =>
            Command<GalleryImage>.Create(async (sender, image) =>
            {
                var that = (GalleryVM)sender.Tag;
                that.View.MoveCurrentToPosition(image.PageId - 1);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(ImagePage), that.gallery.Id);
            }, (sender, image) => image != null));

        public Command<GalleryImage> LoadOriginal => Commands.GetOrAdd(() =>
            Command<GalleryImage>.Create(async (sender, image) =>
            {
                var that = (GalleryVM)sender.Tag;
                try
                {
                    await image.LoadImageAsync(true, ConnectionStrategy.AllFull, true);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(ImagePage));
                }
            }, (sender, image) => image != null && !image.OriginalLoaded));

        public Command<GalleryImage> ReloadImage => Commands.GetOrAdd(() =>
            Command<GalleryImage>.Create(async (sender, image) =>
            {
                var that = (GalleryVM)sender.Tag;
                try
                {
                    if (image.OriginalLoaded)
                    {
                        await image.LoadImageAsync(true, ConnectionStrategy.AllFull, true);
                    }
                    else
                    {
                        await image.LoadImageAsync(true, SettingCollection.Current.GetStrategy(), true);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(ImagePage));
                }
            }, (sender, image) => image != null));

        public Command<SHA1Value> SearchImage => Commands.GetOrAdd(() =>
            Command<SHA1Value>.Create(async (sender, hash) =>
            {
                var that = (GalleryVM)sender.Tag;
                var search = Client.Current.Search("", Category.All, Enumerable.Repeat(hash, 1), that.gallery.GetDisplayTitle());
                var vm = SearchVM.GetVM(search);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }, (sender, hash) => ((GalleryVM)sender.Tag).gallery != null && hash != default));

        public Command SearchUploader => Commands.GetOrAdd(() =>
            Command.Create(async sender =>
            {
                var that = (GalleryVM)sender.Tag;
                var search = Client.Current.Search(that.gallery.Uploader, null, SettingCollection.Current.DefaultSearchCategory);
                var vm = SearchVM.GetVM(search);
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }, sender => ((GalleryVM)sender.Tag).gallery != null));

        private Gallery gallery;

        public Gallery Gallery
        {
            get => gallery;
            private set
            {
                history = null;
                if (View != null)
                {
                    View.CurrentChanged -= View_CurrentChanged;
                }
                View = value?.CreateView();
                if (View != null)
                {
                    View.MoveCurrentToPrevious();
                    View.CurrentChanged += View_CurrentChanged;
                }
                Set(nameof(View), ref gallery, value);
                Save.OnCanExecuteChanged();
                Share.OnCanExecuteChanged();
                AddComment.OnCanExecuteChanged();
                Torrents = null;
                if (value != null)
                {
                    history = new HistoryRecord
                    {
                        Title = value.GetDisplayTitle(),
                        Type = HistoryRecordType.Gallery,
                        Uri = value.GalleryUri,
                    };
                    HistoryDb.Add(history);
                }
            }
        }

        private HistoryRecord history;

        private void View_CurrentChanged(object sender, object e)
        {
            CurrentInfo = null;
            QRCodeResult = null;
            history.Title = gallery.GetDisplayTitle();
            if (View.CurrentItem?.PageUri is Uri pageUri)
            {
                history.Type = HistoryRecordType.Image;
                history.Uri = pageUri;
            }
            else
            {
                history.Type = HistoryRecordType.Gallery;
                history.Uri = gallery.GalleryUri;
            }
            HistoryDb.Update(history);
        }

        public new CollectionView<GalleryImage> View { get; private set; }

        private string currentInfo;
        public string CurrentInfo { get => currentInfo; private set => Set(ref currentInfo, value); }

        private string qrCodeResult;
        private int qrCodeHash;
        public string QRCodeResult
        {
            get => qrCodeResult;
            private set
            {
                Set(ref qrCodeResult, value);
                qrCodeHash = 0;
            }
        }

        public IAsyncAction RefreshInfoAsync()
        {
            var current = (GalleryImage)View.CurrentItem;
            var imageFile = current?.ImageFile;
            if (imageFile is null)
            {
                CurrentInfo = Strings.Resources.Views.ImagePage.ImageFileInfoDefault;
                QRCodeResult = null;
                return AsyncAction.CreateCompleted();
            }
            return Task.Run(async () =>
            {
                var prop = await imageFile.GetBasicPropertiesAsync();
                var imageProp = await imageFile.Properties.GetImagePropertiesAsync();
                CurrentInfo = Strings.Resources.Views.ImagePage.ImageFileInfo(
                    fileType: imageFile.DisplayType,
                    size: Opportunity.UWP.Converters.XBind.ByteSize.ToBinaryString((long)prop.Size),
                    width: imageProp.Width,
                    height: imageProp.Height);
                var newQRHash = prop.Size.GetHashCode() ^ imageFile.Name.GetHashCode();
                if (newQRHash == qrCodeHash)
                {
                    return;
                }

                using (var stream = await imageFile.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var sb = await decoder.GetSoftwareBitmapAsync();
                    var r = new ZXing.QrCode.QRCodeReader();
                    var qr = r.decode(new ZXing.BinaryBitmap(new ZXing.Common.HybridBinarizer(new ZXing.SoftwareBitmapLuminanceSource(sb))));
                    QRCodeResult = qr?.Text;
                    qrCodeHash = newQRHash;
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
        public Command<string> OpenQRCode { get; } = Command<string>.Create(async (sender, code) =>
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

            if (opened)
            {
                return;
            }

            if (!(qrCodeDialog.Content is TextBlock tb))
            {
                tb = new TextBlock { IsTextSelectionEnabled = true, TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap };
                qrCodeDialog.Content = tb;
            }
            tb.Text = code;
            qrCodeDialog.PrimaryButtonCommandParameter = code;
            await Notificator.GetForCurrentView().NotifyAsync(qrCodeDialog);
        }, (sender, code) => code != null);

        private OperationState saveStatus;

        public OperationState SaveStatus
        {
            get => saveStatus;
            set
            {
                Set(ref saveStatus, value);
                Save.OnCanExecuteChanged();
            }
        }

        private double saveProgress;

        public double SaveProgress
        {
            get => saveProgress;
            set => Set(ref saveProgress, value);
        }

        #region Comments

        public AsyncCommand AddComment => Commands.GetOrAdd(() =>
        {
            var ac = AsyncCommand.Create(async (sender) =>
            {
                var that = (GalleryVM)sender.Tag;
                var addComment = ThreadLocalSingleton.GetOrCreate<AddCommentDialog>();
                addComment.Gallery = that.Gallery;
                await addComment.ShowAsync();
            }, sender => ((GalleryVM)sender.Tag).Gallery != null);
            ac.ReentrancyHandler = ReentrancyHandler.LastQueued();
            return ac;
        });

        public IAsyncAction LoadComments()
        {
            return Run(async token =>
            {
                try
                {
                    await gallery.Comments.FetchAsync();
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
                if (torrentfolder is null)
                {
                    torrentfolder = await DownloadsFolder.CreateFolderAsync("Torrents", CreationCollisionOption.GenerateUniqueName);
                    if (ftoken is null)
                    {
                        ftoken = StorageApplicationPermissions.FutureAccessList.Add(torrentfolder);
                    }
                    else
                    {
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace(ftoken, torrentfolder);
                    }
                }
                StatusCollection.Current.TorrentFolderToken = ftoken;
            });
        }

        public Command<TorrentInfo> TorrentDownload => Commands.GetOrAdd(() =>
            Command<TorrentInfo>.Create(async (sender, torrent) =>
            {
                RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TorrentDownloading, null);
                try
                {
                    var file = await torrent.DownloadTorrentAsync(SettingCollection.Current.UsePersonalizedTorrent);
                    if (torrentfolder is null)
                        await loadTorrentFolder();

                    await file.CopyAsync(torrentfolder, file.Name, NameCollisionOption.GenerateUniqueName);
                    if (!await Launcher.LaunchFileAsync(file))
                        await Launcher.LaunchFolderAsync(torrentfolder);

                    RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TorrentDownloaded(torrentfolder.Path), null);
                }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            }, (sender, torrent) => !torrent.IsExpunged));

        public IAsyncAction LoadTorrents()
        {
            return Run(async token =>
            {
                try
                {
                    Torrents = await gallery.FetchTorrnetsAsync();
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
            get => torrents;
            private set
            {
                torrents = value;
                OnPropertyChanged(nameof(Torrents), nameof(TorrentCount));
            }
        }

        public int? TorrentCount => torrents?.Count ?? (gallery is CachedGallery ? null : gallery?.TorrentCount);

        #endregion Torrents
    }
}
