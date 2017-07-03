using Opportunity.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(NextConverter))]
    public class FormatStringConverter : ChainConverter<object, string>
    {
        protected override string ConvertImpl(object value, object parameter, string language)
        {
            var format = (string)null;
            if (parameter == null)
                return value.ToString();
            if (string.IsNullOrEmpty(format = Strings.Resources.GetValue(parameter.ToString())))
            {
                System.Diagnostics.Debug.WriteLine($"Can't find resource: {parameter}", "Localization");
                return value.ToString();
            }

            return string.Format(CultureInfo.CurrentCulture, format, value);
        }

        protected override object ConvertBackImpl(string value, object parameter, string language)
        {
            return value;
        }
    }
}
