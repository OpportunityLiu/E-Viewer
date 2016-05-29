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

    public class GalleryVM : ViewModelBase
    {
        private static CacheStorage<long, GalleryVM> Cache
        {
            get;
        } = new CacheStorage<long, GalleryVM>(id => new GalleryVM(id), 25);

        private static CacheStorage<long, Gallery> GalleryCache
        {
            get;
        } = new CacheStorage<long, Gallery>(id => Gallery.TryLoadGalleryAsync(id).AsTask().Result, 25);

        public static void AddGallery(Gallery gallery)
        {
            GalleryCache.Add(gallery.Id, gallery);
        }

        public static GalleryVM GetVM(long parameter)
        {
            return Cache.Get(parameter);
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
        }

        private void Image_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(GalleryImage.OriginalLoaded))
                LoadOriginal.RaiseCanExecuteChanged();
        }

        public GalleryVM(long parameter)
            : this()
        {
            gallery = GalleryCache.Get(parameter);
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

        private Gallery gallery;

        public Gallery Gallery
        {
            get
            {
                return gallery;
            }
            private set
            {
                Set(ref gallery, value);
            }
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

        public IAsyncAction RefreshInfoAsync()
        {
            return Run(async token =>
            {
                var current = GetCurrent();
                if(current?.Image == null)
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
    }
}
