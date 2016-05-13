using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using System.Threading;


namespace ExClient
{
    class CachedGallery : Gallery
    {
        private GalleryCache cache;

        public CachedGallery(GalleryCache cache, Client owner)
            : base(cache.Id, cache.Token, 0)
        {
            this.cache = cache;
            ArchiverKey = cache.ArchiverKey;
            Available = cache.Available;
            Category = (Category)cache.Category;
            Expunged = cache.Expunged;
            FileSize = cache.FileSize;
            Owner = owner;
            Posted = DateTimeOffset.FromUnixTimeSeconds(cache.Posted);
            Rating = cache.Rating;
            RecordCount = cache.RecordCount;
            Tags = new ReadOnlyCollection<string>(cache.Tags);
            Title = cache.Title;
            TitleJpn = cache.TitleJpn;
            Uploader = cache.Uploader;
            PageCount = RecordCount / 10 + 1;
        }

        internal IAsyncAction InitAsync()
        {
            return Run(async token =>
            {
                BitmapImage thumb;
                var thumbFile = await CacheHelper.LoadFileAsync(Id.ToString(), thumbFileName);
                if(thumbFile == null)
                    thumb = new BitmapImage(new Uri(cache.Thumb));
                else
                    using(var source = await thumbFile.OpenReadAsync())
                    {
                        thumb = new BitmapImage();
                        await thumb.SetSourceAsync(source);
                    }
                this.Thumb = thumb;
            });
        }

        private int loadedCount = 0;

        protected override IAsyncOperation<uint> LoadPage(int pageIndex)
        {
            return Run(async token =>
            {
                uint i = 0;
                var max = (pageIndex + 1) * 10;
                while(this.loadedCount < this.cache.ImageKeys.Count && this.loadedCount < max)
                {
                    this.Add(await GalleryImage.LoadCachedImageAsync(this, this.loadedCount + 1, this.cache.ImageKeys[this.loadedCount]));
                    this.loadedCount++;
                    i++;
                }
                return i;
            });
        }

        private static IAsyncActionWithProgress<SaveGalleryProgress> emptySave = Run<SaveGalleryProgress>(async (token, progress) =>
        {
            await Task.Yield();
            return;
        });

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAction
        {
            get
            {
                return emptySave;
            }
        }

        public override IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync()
        {
            return emptySave;
        }
    }
}
