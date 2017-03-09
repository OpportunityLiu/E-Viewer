using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class LengthConverter : ChainConverter
    {
        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType);
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType);
        }

        private static object convert(object value, Type targetType)
        {
            var result = 0d;
            switch(value)
            {
            case double vd :
                result = vd;
                break;
            case GridLength vgl:
                result = vgl.IsAbsolute ? vgl.Value : double.NaN;
                break;
            default:
                result = System.Convert.ToDouble(value);
                break;
            }
            if(targetType == typeof(double))
                return result;
            if(targetType == typeof(GridLength))
                return new GridLength(result);
            return DependencyProperty.UnsetValue;
        }
    }
}
