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
            var v = value as double?;
            if(v == null)
            {
                if(value is GridLength)
                {
                    var v2 = (GridLength)value;
                    if(v2.IsAbsolute)
                        v = v2.Value;
                    else
                        v = double.NaN;
                }
            }

            if(v == null)
                throw new InvalidOperationException();
            var d = v.Value;
            if(targetType == typeof(double))
                return d;
            if(targetType == typeof(GridLength))
                return new GridLength(d);
            throw new InvalidOperationException();
        }
    }
}
