using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;

namespace ExViewer.Converters
{
    public static class ConvertHelper
    {
        public static T ChangeType<T>(object value)
        {
            if(value is T v)
                return v;
            if(value == null)
                return default(T);
            return (T)XamlBindingHelper.ConvertValue(typeof(T), value);
        }

        public static object ChangeType(object value, Type targetType)
        {
            if(value == null)
                return null;
            return XamlBindingHelper.ConvertValue(targetType, value);
        }
    }
}
