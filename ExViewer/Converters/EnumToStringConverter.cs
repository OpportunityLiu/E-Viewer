using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class EnumToStringConverter : ChainConverter
    {
        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, value.ToString());
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }
    }
}
