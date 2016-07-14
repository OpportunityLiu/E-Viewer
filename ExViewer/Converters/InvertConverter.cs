using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public sealed class InvertConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            return InnerConverter.ConvertBack(value, targetType, parameter, language);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return InnerConverter.Convert(value, targetType, parameter, language);
        }
    }
}
