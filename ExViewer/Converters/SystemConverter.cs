using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class SystemConverter:ChainConverter
    {
        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return ChangeType(value, targetType);
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            return ChangeType(value, targetType);
        }

        public static object ChangeType(object value,Type targetType)
        {
            if(targetType.IsInstanceOfType(value))
                return value;
            return System.Convert.ChangeType(value, targetType);
        }
    }
}
