using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Models
{
    class ImageModel
    {
        public string ImageKey
        {
            get;
            set;
        }

        public bool OriginalLoaded
        {
            get;
            set;
        }
        
        public long OwnerId
        {
            get;
            set;
        }
        
        public GalleryModel Owner
        {
            get;
            set;
        }

        public int PageId
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public ImageModel Update(GalleryImage galleryImage)
        {
            this.ImageKey = galleryImage.ImageKey;
            this.FileName = galleryImage.ImageFile.Name;
            this.OriginalLoaded = galleryImage.OriginalLoaded;
            this.OwnerId = galleryImage.Owner.Id;
            this.PageId = galleryImage.PageId;
            return this;
        }
    }
}
