using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient;
using ExClient.Galleries;

namespace ExViewer.Views
{
    internal interface IImagePageImageView : INotifyPropertyChanged
    {
        GalleryImage Image
        {
            get;
        }
    }
}
