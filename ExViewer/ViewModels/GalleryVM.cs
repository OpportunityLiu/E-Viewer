using ExClient;
using ExViewer.Settings;
using ExViewer.Views;
using Newtonsoft.Json;
using System;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Graphics.Imaging;
using ExClient.Api;
using Windows.ApplicationModel.DataTransfer;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Helpers;
using ExClient.Tagging;
using ExClient.Galleries;
using ExClient.Galleries.Metadata;

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
        private static CacheStorage<GalleryInfo, GalleryVM> Cache
        {
            get;
        } = new CacheStorage<GalleryInfo, GalleryVM>(gi => Run(async token => new GalleryVM(await gi.FetchGalleryAsync())), 25, new GalleryInfoComparer());

        private class GalleryInfoComparer : IEqualityComparer<GalleryInfo>
        {
            public bool Equals(GalleryInfo x, GalleryInfo y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(GalleryInfo obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public static GalleryVM GetVM(Gallery gallery)
        {
            var gi = new GalleryInfo(gallery.Id, gallery.Token);
            if (Cache.TryGet(gi, out var vm))
            {
                vm.Gallery = gallery;
                if (gallery.Count <= vm.currentIndex)
                    vm.currentIndex = -1;
            }
            else
            {
                vm = new GalleryVM(gallery);
                Cache.Add(gi, vm);
            }
            return vm;
        }

        public static IAsyncOperation<GalleryVM> GetVMAsync(long parameter)
        {
            return Cache.GetAsync(new GalleryInfo(parameter, 0));
        }

        public static IAsyncOperation<GalleryVM> GetVMAsync(GalleryInfo gInfo)
        {
            return Cache.GetAsync(gInfo);
        }

        public GalleryImage GetCurrent()
        {
            try
            {
                return this.Gallery[this.currentIndex];
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private GalleryVM()
        {
            this.Share = new Command<GalleryImage>(async image =>
            {
                if (Helpers.ShareHandler.IsShareSupported)
                {
                    Helpers.ShareHandler.Share(async (s, e) =>
                    {
                        var deferral = e.Request.GetDeferral();
                        try
                        {
                            var data = e.Request.Data;
                            var gallery = this.gallery;
                            data.Properties.Title = gallery.GetDisplayTitle();
                            data.Properties.Description = gallery.GetSecondaryTitle();
                            if (image == null)
                            {
                                var ms = new InMemoryRandomAccessStream();
                                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms);
                                encoder.SetSoftwareBitmap(gallery.Thumb);
                                await encoder.FlushAsync();
                                data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(ms);
                                var firstImage = gallery.FirstOrDefault()?.ImageFile;
                                if (firstImage != null)
                                    data.SetBitmap(RandomAccessStreamReference.CreateFromFile(firstImage));
                                data.Properties.ContentSourceWebLink = gallery.GalleryUri;
                                data.SetWebLink(gallery.GalleryUri);
                                data.SetText(gallery.GalleryUri.ToString());
                                data.RequestedOperation = DataPackageOperation.Move;
                                data.Properties.FileTypes.Add(StandardDataFormats.StorageItems);
                                data.SetDataProvider(StandardDataFormats.StorageItems, async request =>
                                {
                                    var d = request.GetDeferral();
                                    try
                                    {
                                        var makeCopy = SavedVM.GetCopyOf(gallery);
                                        request.SetData(Enumerable.Repeat(await makeCopy, 1));
                                    }
                                    finally { d.Complete(); }
                                });
                            }
                            else
                            {
                                data.RequestedOperation = DataPackageOperation.Copy;
                                if (image.ImageFile != null)
                                {
                                    var view = RandomAccessStreamReference.CreateFromFile(image.ImageFile);
                                    data.SetBitmap(view);
                                    data.Properties.Thumbnail = view;
                                    data.SetStorageItems(Enumerable.Repeat(image.ImageFile, 1), true);
                                }
                                data.Properties.ContentSourceWebLink = image.PageUri;
                                data.SetWebLink(image.PageUri);
                                data.SetText(image.PageUri.ToString());
                            }
                        }
                        finally
                        {
                            deferral.Complete();
                        }
                    });
                }
                else
                {
                    if (image == null)
                        await Launcher.LaunchUriAsync(this.gallery.GalleryUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                    else
                        await Launcher.LaunchUriAsync(image.PageUri, new LauncherOptions { IgnoreAppUriHandlers = true });
                }
            }, image => this.gallery != null);
            this.Save = new Command(() =>
            {
                this.SaveStatus = OperationState.Started;
                this.SaveProgress = -1;
                var task = this.gallery.SaveGalleryAsync(SettingCollection.Current.GetStrategy());
                task.Progress = (sender, e) =>
                {
                    this.SaveProgress = e.ImageLoaded / (double)e.ImageCount;
                };
                task.Completed = (sender, e) =>
                {
                    switch (e)
                    {
                    case AsyncStatus.Canceled:
                    case AsyncStatus.Error:
                        this.SaveStatus = OperationState.Failed;
                        RootControl.RootController.SendToast(sender.ErrorCode, null);
                        break;
                    case AsyncStatus.Completed:
                        this.SaveStatus = OperationState.Completed;
                        break;
                    case AsyncStatus.Started:
                        this.SaveStatus = OperationState.Started;
                        break;
                    }
                    this.SaveProgress = 1;
                };
            }, () =>
            {
                if (this.SaveStatus == OperationState.Started)
                    return false;
                if (this.gallery is SavedGallery)
                    return false;
                return true;
            });
            this.OpenImage = new Command<GalleryImage>(image =>
            {
                this.CurrentIndex = image.PageId - 1;
                RootControl.RootController.Frame.Navigate(typeof(ImagePage), this.gallery.Id);
            });
            this.LoadOriginal = new Command<GalleryImage>(async image =>
            {
                image.PropertyChanged += this.Image_PropertyChanged;
                await image.LoadImageAsync(true, ConnectionStrategy.AllFull, false);
                image.PropertyChanged -= this.Image_PropertyChanged;
            }, image => image != null && !image.OriginalLoaded);
            this.ReloadImage = new Command<GalleryImage>(async image =>
            {
                image.PropertyChanged += this.Image_PropertyChanged;
                if (image.OriginalLoaded)
                    await image.LoadImageAsync(true, ConnectionStrategy.AllFull, false);
                else
                    await image.LoadImageAsync(true, SettingCollection.Current.GetStrategy(), false);
                image.PropertyChanged -= this.Image_PropertyChanged;
            }, image => image != null);
            this.TorrentDownload = new Command<TorrentInfo>(async torrent =>
            {
                RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TorrentDownloading, null);
                try
                {
                    var file = await torrent.LoadTorrentAsync();
                    var dfile = await DownloadsFolder.CreateFileAsync(file.Name, CreationCollisionOption.GenerateUniqueName);
                    var data = await FileIO.ReadBufferAsync(file);
                    await FileIO.WriteBufferAsync(dfile, data);
                    await Launcher.LaunchFileAsync(dfile);
                    RootControl.RootController.SendToast(string.Format(Strings.Resources.Views.GalleryPage.TorrentDownloaded, dfile.Path), null);
                }
                catch (Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            }, torrent => torrent != null && torrent.TorrentUri != null);
            this.SearchTag = new Command<Tag>(tag =>
            {
                var vm = SearchVM.GetVM(tag.Search());
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery);
            }, tag => tag.Content != null);
        }

        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GalleryImage.OriginalLoaded))
                this.LoadOriginal.RaiseCanExecuteChanged();
        }

        private GalleryVM(Gallery gallery)
            : this()
        {
            this.Gallery = gallery;
        }

        public Command<GalleryImage> Share { get; }

        public Command Save { get; }

        public Command<GalleryImage> OpenImage { get; }

        public Command<GalleryImage> LoadOriginal { get; }

        public Command<GalleryImage> ReloadImage { get; }

        public Command<TorrentInfo> TorrentDownload { get; }

        public Command<Tag> SearchTag { get; }

        private static readonly EhWikiDialog ewd = new EhWikiDialog();
        public AsyncCommand<Tag> ShowTagDefination { get; }
            = new AsyncCommand<Tag>(async tag =>
            {
                ewd.WikiTag = tag;
                await ewd.ShowAsync();
            }, tag => tag.Content != null);

        public Command<Tag> CopyTag { get; }
            = new Command<Tag>(tag =>
            {
                var data = new DataPackage();
                data.SetText(tag.Content);
                Clipboard.SetContent(data);
                RootControl.RootController.SendToast(Strings.Resources.Views.GalleryPage.TagCopied, typeof(GalleryPage));
            }, tag => tag.Content != null);

        private Gallery gallery;

        public Gallery Gallery
        {
            get => this.gallery;
            private set
            {
                if (this.gallery != null)
                    this.gallery.LoadMoreItemsException -= this.Gallery_LoadMoreItemsException;
                Set(ref this.gallery, value);
                if (this.gallery != null)
                    this.gallery.LoadMoreItemsException += this.Gallery_LoadMoreItemsException;
                this.Save.RaiseCanExecuteChanged();
                this.Share.RaiseCanExecuteChanged();
                this.Torrents = null;
            }
        }

        private void Gallery_LoadMoreItemsException(IncrementalLoadingCollection<GalleryImage> sender, LoadMoreItemsExceptionEventArgs args)
        {
            RootControl.RootController.SendToast(args.Exception, typeof(GalleryPage));
            args.Handled = true;
        }

        private int currentIndex = -1;

        public int CurrentIndex
        {
            get => this.currentIndex;
            set => Set(ref this.currentIndex, value);
        }

        private string currentInfo;

        public string CurrentInfo
        {
            get => this.currentInfo;
            private set => Set(ref this.currentInfo, value);
        }

        public IAsyncAction RefreshInfoAsync()
        {
            return Run(async token =>
            {
                var current = GetCurrent();
                if (current?.ImageFile == null)
                {
                    this.CurrentInfo = Strings.Resources.Views.ImagePage.ImageFileInfoDefault;
                    return;
                }
                var prop = await current.ImageFile.GetBasicPropertiesAsync();
                var imageProp = await current.ImageFile.Properties.GetImagePropertiesAsync();
                this.CurrentInfo = string.Format(Strings.Resources.Views.ImagePage.ImageFileInfo, current.ImageFile.Name,
                    Opportunity.Converters.ByteSizeToStringConverter.ByteSizeToString(prop.Size, Opportunity.Converters.UnitPrefix.Binary),
                    imageProp.Width.ToString(), imageProp.Height.ToString());
            });
        }

        private OperationState saveStatus;

        public OperationState SaveStatus
        {
            get => this.saveStatus;
            set
            {
                Set(ref this.saveStatus, value);
                this.Save.RaiseCanExecuteChanged();
            }
        }

        private double saveProgress;

        public double SaveProgress
        {
            get => this.saveProgress;
            set => Set(ref this.saveProgress, value);
        }

        #region Comments

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
                DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(Torrents));
                    RaisePropertyChanged(nameof(TorrentCount));
                });
            }
        }

        public int? TorrentCount => this.torrents?.Count ?? (this.gallery is SavedGallery ? null : this.gallery?.TorrentCount);

        #endregion Torrents
    }
}
