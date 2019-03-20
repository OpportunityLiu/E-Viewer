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
            GalleryModelId = toCache.Id;
            Available = toCache.Available;
            Token = toCache.Token.Value;
            Title = toCache.Title;
            TitleJpn = toCache.TitleJpn;
            Category = toCache.Category;
            Uploader = toCache.Uploader;
            Posted = toCache.Posted;
            FileSize = toCache.FileSize;
            Expunged = toCache.Expunged;
            Rating = toCache.Rating.AverageScore;
            if (toCache.Tags is null || toCache.Tags.Items.Count == 0)
            {
                Tags = "[]";
            }
            else
            {
                Tags = JsonConvert.SerializeObject(toCache.Tags.Items.Select(tag => tag.Content.ToString()));
            }

            RecordCount = toCache.Count;
            ThumbUri = toCache.ThumbUri.ToString();
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
            set => posted = value.ToUnixTimeMilliseconds();
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
