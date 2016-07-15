using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class DateTimeToStringConverter : ChainConverter
    {
        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            var r = (string)null;
            if(value == null)
            {
                r = string.Empty;
            }
            else
            {
                var currentType = value.GetType();
                if(currentType == typeof(DateTimeOffset))
                {
                    var date = (DateTimeOffset)value;
                    r = date.LocalDateTime.ToString(CultureInfo.CurrentCulture);
                }
                else if(currentType == typeof(DateTime))
                {
                    var date = (DateTime)value;
                    r = date.ToString(CultureInfo.CurrentCulture);
                }
            }
            return r;
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            if(targetType == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value.ToString());
            else if(targetType == typeof(DateTime))
                return DateTime.Parse(value.ToString());
            throw new NotImplementedException();
        }
    }
}
