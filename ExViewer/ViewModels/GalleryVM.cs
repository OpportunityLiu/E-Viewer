using ExClient;
using ExViewer.Settings;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
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

namespace ExViewer.ViewModels
{
    public enum OperationState
    {
        NotStarted,
        Started,
        Failed,
        Completed
    }

    public class TagList : List<Tag>
    {
        public TagList(IEnumerable<Tag> items) : base(items) { }

        public NameSpace NameSpace => this.FirstOrDefault()?.NameSpace ?? NameSpace.Misc;
    }

    public class GalleryVM : ViewModelBase
    {
        private static CacheStorage<long, GalleryVM> Cache
        {
            get;
        } = new CacheStorage<long, GalleryVM>(id => Run(async token => new GalleryVM(await Gallery.TryLoadGalleryAsync(id))), 25);

        public static void AddGallery(Gallery gallery)
        {
            GalleryVM vm;
            if(Cache.TryGet(gallery.Id, out vm))
                vm.Gallery = gallery;
            else
                Cache.Add(gallery.Id, new GalleryVM(gallery));
        }

        public static IAsyncOperation<GalleryVM> GetVMAsync(long parameter)
        {
            return Cache.GetAsync(parameter);
        }

        public GalleryImage GetCurrent()
        {
            try
            {
                return Gallery[currentIndex];
            }
            catch(ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private GalleryVM()
        {
            OpenInBrowser = new RelayCommand<GalleryImage>(async image =>
            {
                if(image == null)
                    await Launcher.LaunchUriAsync(gallery.GalleryUri);
                else
                    await Launcher.LaunchUriAsync(image.PageUri);
            }, image => gallery != null);
            OpenInExplorer = new RelayCommand(async () => await Launcher.LaunchFolderAsync(gallery.GalleryFolder), () => gallery != null);
            Save = new RelayCommand(() =>
            {
                var task = gallery.SaveGalleryAsync(SettingCollection.Current.GetStrategy());
                SaveStatus = OperationState.Started;
                task.Progress = (sender, e) =>
                {
                    SaveProgress = e.ImageLoaded / (double)e.ImageCount;
                };
                task.Completed = (sender, e) =>
                {
                    switch(e)
                    {
                    case AsyncStatus.Canceled:
                    case AsyncStatus.Error:
                        SaveStatus = OperationState.Failed;
                        break;
                    case AsyncStatus.Completed:
                        SaveStatus = OperationState.Completed;
                        break;
                    case AsyncStatus.Started:
                        SaveStatus = OperationState.Started;
                        break;
                    }
                    SaveProgress = 1;
                };
            }, () => SaveStatus != OperationState.Started && !(Gallery is CachedGallery));
            OpenImage = new RelayCommand<GalleryImage>(image =>
            {
                CurrentIndex = image.PageId - 1;
                RootControl.RootController.Frame.Navigate(typeof(ImagePage), gallery.Id);
            });
            LoadOriginal = new RelayCommand<GalleryImage>(async image =>
            {
                image.PropertyChanged += Image_PropertyChanged;
                await image.LoadImageAsync(true, ConnectionStrategy.AllFull, false);
                image.PropertyChanged -= Image_PropertyChanged;
            }, image => image != null && !image.OriginalLoaded);
            ReloadImage = new RelayCommand<GalleryImage>(async image =>
            {
                if(image.OriginalLoaded)
                    await image.LoadImageAsync(true, ConnectionStrategy.AllFull, false);
                else
                    await image.LoadImageAsync(true, SettingCollection.Current.GetStrategy(), false);
            });
            TorrentDownload = new RelayCommand<TorrentInfo>(async torrent =>
            {
                var file = await torrent.LoadTorrentAsync();
                await Launcher.LaunchFileAsync(file);
            }, torrent => torrent != null);
            GoToDefination = new RelayCommand<Tag>(async tag =>
            {
                await Launcher.LaunchUriAsync(tag.TagDefinationUri);
            }, tag => tag != null);
            SearchTag = new RelayCommand<Tag>(tag =>
            {
                var param = SearchVM.Cache.AddSearchResult(tag.Search());
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), param);
            }, tag => tag != null);
        }

        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(GalleryImage.OriginalLoaded))
                LoadOriginal.RaiseCanExecuteChanged();
        }

        private GalleryVM(Gallery gallery)
            : this()
        {
            this.Gallery = gallery;
        }

        public RelayCommand<GalleryImage> OpenInBrowser
        {
            get;
        }

        public RelayCommand OpenInExplorer
        {
            get;
        }

        public RelayCommand Save
        {
            get;
        }

        public RelayCommand<GalleryImage> OpenImage
        {
            get;
        }

        public RelayCommand<GalleryImage> LoadOriginal
        {
            get;
        }

        public RelayCommand<GalleryImage> ReloadImage
        {
            get;
        }

        public RelayCommand<TorrentInfo> TorrentDownload
        {
            get;
        }

        public RelayCommand<Tag> GoToDefination
        {
            get;
        }

        public RelayCommand<Tag> SearchTag
        {
            get;
        }

        private Gallery gallery;

        public Gallery Gallery
        {
            get
            {
                return gallery;
            }
            private set
            {
                if(gallery != null)
                    gallery.LoadMoreItemsException -= Gallery_LoadMoreItemsException;
                Set(ref gallery, value);
                if(gallery != null)
                    gallery.LoadMoreItemsException += Gallery_LoadMoreItemsException;
                Save.RaiseCanExecuteChanged();
                OpenInBrowser.RaiseCanExecuteChanged();
                OpenInExplorer.RaiseCanExecuteChanged();
                Torrents = null;
                RaisePropertyChanged(nameof(SortedTags));
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
            get
            {
                return currentIndex;
            }
            set
            {
                Set(ref currentIndex, value);
            }
        }

        private string currentInfo;

        public string CurrentInfo
        {
            get
            {
                return currentInfo;
            }
            private set
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => Set(ref currentInfo, value));
            }
        }

        public SortedSet<TagList> SortedTags
        {
            get
            {
                if(gallery == null)
                    return null;
                var query = from tag in gallery.Tags
                            group tag by tag.NameSpace into taggroup
                            select new TagList(taggroup);
                return new SortedSet<TagList>(query, Comparer<TagList>.Create((x, y) => x.NameSpace - y.NameSpace));
            }
        }

        public IAsyncAction RefreshInfoAsync()
        {
            return Run(async token =>
            {
                var current = GetCurrent();
                if(current?.ImageFile == null)
                {
                    CurrentInfo = null;
                    return;
                }
                var prop = await current.ImageFile.GetBasicPropertiesAsync();
                var imageProp = await current.ImageFile.Properties.GetImagePropertiesAsync();
                CurrentInfo = $@"File name: {current.ImageFile.Name}
Size: {ByteToString(prop.Size)}
Dimensions: {imageProp.Width} × {imageProp.Height}";
            });
        }

        private static string ByteToString(ulong byteCount)
        {
            if(byteCount < 1024ul)
                return $"{byteCount} B";
            double bInK = byteCount / 1024.0;
            if(byteCount < 1024ul * 1024ul)
                return $"{bInK:F2} KiB";
            double bInM = bInK / 1024.0;
            if(byteCount < 1024ul * 1024ul * 1024ul)
                return $"{bInM:F2} MiB";
            double bInG = bInM / 1024.0;
            if(byteCount < 1024ul * 1024ul * 1024ul * 1024ul)
                return $"{bInG:F2} GiB";
            double bInT = bInG / 1024.0;
            return $"{bInT:F2} TiB";
        }

        private OperationState saveStatus;

        public OperationState SaveStatus
        {
            get
            {
                return saveStatus;
            }
            set
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Set(ref saveStatus, value);
                    Save.RaiseCanExecuteChanged();
                });
            }
        }

        private double saveProgress;

        public double SaveProgress
        {
            get
            {
                return saveProgress;
            }
            set
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() => Set(ref saveProgress, value));
            }
        }

        #region Comments

        public IAsyncAction LoadComments()
        {
            return Run(async token =>
            {
                try
                {
                    Comments = await gallery.LoadCommentsAsync();
                }
                catch(Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            });
        }

        private List<Comment> comments;

        public List<Comment> Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    RaisePropertyChanged(nameof(Comments));
                    RaisePropertyChanged(nameof(CommentCount));
                });
            }
        }

        public int? CommentCount => comments?.Count;

        #endregion Comments

        #region Torrents

        public IAsyncAction LoadTorrents()
        {
            return Run(async token =>
            {
                try
                {
                    Torrents = await gallery.LoadTorrnetsAsync();
                }
                catch(Exception ex)
                {
                    RootControl.RootController.SendToast(ex, typeof(GalleryPage));
                }
            });
        }

        private List<TorrentInfo> torrents;

        public List<TorrentInfo> Torrents
        {
            get
            {
                return torrents;
            }
            set
            {
                torrents = value;
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    RaisePropertyChanged(nameof(Torrents));
                    RaisePropertyChanged(nameof(TorrentCount));
                });
            }
        }

        public int? TorrentCount => torrents?.Count ?? (gallery is CachedGallery ? null : gallery?.TorrentCount);

        #endregion Torrents
    }
}
