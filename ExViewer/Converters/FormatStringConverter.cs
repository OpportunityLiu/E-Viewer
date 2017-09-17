using Opportunity.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    public sealed class FormatStringConverter : ValueConverter<object, string>
    {
        public override string Convert(object value, object parameter, string language)
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

        public override object ConvertBack(string value, object parameter, string language)
        {
            return value;
        }
    }
}
