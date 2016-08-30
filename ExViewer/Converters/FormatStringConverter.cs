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
            var format = (string)null;
            if(parameter == null)
                return value.ToString();
            if(string.IsNullOrEmpty(format = LocalizedStrings.Resources.GetString(parameter.ToString())))
            {
                System.Diagnostics.Debug.WriteLine($"Can't find resource: {parameter}", "Localization");
                return value.ToString();
            }

            return string.Format(CultureInfo.CurrentCulture, format, value);
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
