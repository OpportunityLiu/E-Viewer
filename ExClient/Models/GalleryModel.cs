using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ExClient.Models
{
    class GalleryModel
    {
        internal GalleryModel()
        {
        }

        internal GalleryModel Update(Gallery toCache)
        {
            this.Id = toCache.Id;
            this.Available = toCache.Available;
            this.ArchiverKey = toCache.ArchiverKey;
            this.Token = toCache.Token;
            this.Title = toCache.Title;
            this.TitleJpn = toCache.TitleJpn;
            this.Category = toCache.Category;
            this.Uploader = toCache.Uploader;
            this.Posted = toCache.Posted;
            this.FileSize = toCache.FileSize;
            this.Expunged = toCache.Expunged;
            this.Rating = toCache.Rating;
            this.Tags = JsonConvert.SerializeObject(toCache.Tags.Select(tag => tag.ToString()));
            this.RecordCount = toCache.RecordCount;
            this.ThumbUri = toCache.ThumbUri.ToString();
            return this;
        }

        public long Id
        {
            get; set;
        }

        public bool Available
        {
            get; set;
        }

        public string Token
        {
            get; set;
        }

        public string ArchiverKey
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public string TitleJpn
        {
            get; set;
        }

        public Category Category
        {
            get; set;
        }

        public string Uploader
        {
            get; set;
        }

        public DateTimeOffset Posted
        {
            get; set;
        }

        public long FileSize
        {
            get; set;
        }

        public bool Expunged
        {
            get; set;
        }

        public double Rating
        {
            get; set;
        }

        public string ThumbUri
        {
            get; set;
        }

        public int RecordCount
        {
            get; set;
        }

        public string Tags
        {
            get; set;
        }

        public List<ImageModel> Images
        {
            get; set;
        }
    }
}
