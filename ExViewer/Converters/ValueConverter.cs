using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ExViewer.Converters
{
    public abstract class ValueConverter : DependencyObject, IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, string language);
        public abstract object ConvertBack(object value, Type targetType, object parameter, string language);
    }
}
