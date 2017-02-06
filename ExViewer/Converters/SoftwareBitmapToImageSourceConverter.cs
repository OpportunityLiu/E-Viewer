using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace ExViewer.Converters
{
    public class SoftwareBitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sb = value as SoftwareBitmap;
            if(sb == null)
                return null;
            var image = new SoftwareBitmapSource();
            var ignore = image.SetBitmapAsync(sb);
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
