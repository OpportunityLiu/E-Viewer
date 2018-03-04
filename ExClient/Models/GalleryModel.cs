using ExClient.Galleries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Models
{
    class GalleryModel
    {
        internal GalleryModel Update(Gallery toCache)
        {
            this.GalleryModelId = toCache.ID;
            this.Available = toCache.Available;
            this.Token = toCache.Token;
            this.Title = toCache.Title;
            this.TitleJpn = toCache.TitleJpn;
            this.Category = toCache.Category;
            this.Uploader = toCache.Uploader;
            this.Posted = toCache.Posted;
            this.FileSize = toCache.FileSize;
            this.Expunged = toCache.Expunged;
            this.Rating = toCache.Rating.AverageScore;
            if (toCache.Tags == null || toCache.Tags.Items.Count == 0)
                this.Tags = "[]";
            else
                this.Tags = JsonConvert.SerializeObject(toCache.Tags.Items.Select(tag => tag.Content.ToString()));
            this.RecordCount = toCache.Count;
            this.ThumbUri = toCache.ThumbUri.ToString();
            return this;
        }

        public long GalleryModelId { get; set; }

        public bool Available { get; set; }

        public ulong Token { get; set; }

        public string Title { get; set; }

        public string TitleJpn { get; set; }

        public Category Category { get; set; }

        public string Uploader { get; set; }

        public long posted;
        public DateTimeOffset Posted
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(posted);
            set => this.posted = value.ToUnixTimeMilliseconds();
        }

        public long FileSize { get; set; }

        public bool Expunged { get; set; }

        public double Rating { get; set; }

        public string ThumbUri { get; set; }

        public int RecordCount { get; set; }

        public string Tags { get; set; }

        public IList<GalleryImageModel> Images { get; set; }
    }
}
