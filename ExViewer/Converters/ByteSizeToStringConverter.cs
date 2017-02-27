using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class ByteSizeToStringConverter : ChainConverter
    {
        private static readonly string[] unitsMetric = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private static readonly string[] unitsBinary = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB" };

        public UnitPrefix UnitPrefix
        {
            get { return (UnitPrefix)GetValue(UnitPrefixProperty); }
            set { SetValue(UnitPrefixProperty, value); }
        }

        public static readonly DependencyProperty UnitPrefixProperty =
            DependencyProperty.Register(nameof(UnitPrefix), typeof(UnitPrefix), typeof(ByteSizeToStringConverter), new PropertyMetadata(UnitPrefix.Binary));

        public string OutOfRangeValue
        {
            get { return (string)GetValue(OutOfRangeValueProperty); }
            set { SetValue(OutOfRangeValueProperty, value); }
        }

        public static readonly DependencyProperty OutOfRangeValueProperty =
            DependencyProperty.Register("OutOfRangeValue", typeof(string), typeof(ByteSizeToStringConverter), new PropertyMetadata("???", OutOfRangeValuePropertyChangedCallback));

        private static void OutOfRangeValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
                throw new ArgumentNullException(nameof(OutOfRangeValue));
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            var size = System.Convert.ToDouble(value);
            try
            {
                return ByteSizeToString(size, this.UnitPrefix);
            }
            catch(ArgumentException)
            {
                return OutOfRangeValue;
            }
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            var sizeStr = value.ToString();
            try
            {
                return StringToByteSize(sizeStr, this.UnitPrefix);
            }
            catch(Exception)
            {
                return DependencyProperty.UnsetValue;
            }
        }

        private static void getUnits(out string[] units, out double powerBase, UnitPrefix unitPrefix)
        {
            if(unitPrefix == UnitPrefix.Metric)
            {
                units = unitsMetric;
                powerBase = 1000;
            }
            else
            {
                units = unitsBinary;
                powerBase = 1024;
            }
        }

        private static string sizeFormat = "0" + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator + "000";

        public static string ByteSizeToString(double size, UnitPrefix unitPrefix)
        {
            if(size < 0 || double.IsNaN(size))
                throw new ArgumentOutOfRangeException(nameof(size));
            string[] units;
            double powerBase;
            getUnits(out units, out powerBase, unitPrefix);
            foreach(var unit in units)
            {
                if(size < 1000)
                {
                    return $"{size.ToString(sizeFormat).Substring(0, 5)} {unit}";
                }
                size /= powerBase;
            }
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        public static double StringToByteSize(string sizeStr, UnitPrefix unitPrefix)
        {
            if(string.IsNullOrEmpty(sizeStr))
                throw new ArgumentNullException(nameof(sizeStr));
            sizeStr = sizeStr.Trim();
            string[] units;
            double powerBase;
            getUnits(out units, out powerBase, unitPrefix);
            for(int i = 0; i < units.Length; i++)
            {
                if(sizeStr.EndsWith(units[i], StringComparison.OrdinalIgnoreCase))
                {
                    var sizeNumStr = sizeStr.Substring(0, sizeStr.Length - units[i].Length);
                    var sizeNum = double.Parse(sizeNumStr);
                    return sizeNum * Math.Pow(powerBase, i);
                }
            }
            throw new FormatException(Strings.Resources.ParseByteException);
        }
    }

    public enum UnitPrefix
    {
        Binary,
        Metric
    }
}
