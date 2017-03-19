using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization.DateTimeFormatting;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class DateTimeToStringConverter : ChainConverter
    {
        private static DateTimeFormatter formatter = new DateTimeFormatter("shortdate shorttime");

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            if(value == null)
            {
                return "";
            }
            else
            {
                DateTimeOffset d;
                if(value is DateTimeOffset dto)
                {
                    d = dto.ToLocalTime();
                }
                else if(value is DateTime dt)
                {
                    d = new DateTimeOffset(dt).ToLocalTime();
                }
                else
                {
                    return "";
                }
                return formatter.Format(d);
            }
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
