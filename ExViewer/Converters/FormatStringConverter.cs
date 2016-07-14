using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class FormatStringConverter : ChainConverter
    {
        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            return string.Format(CultureInfo.CurrentCulture, parameter?.ToString() ?? "{0}", value);
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
