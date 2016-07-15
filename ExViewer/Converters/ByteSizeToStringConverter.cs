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

        public BytePresentation Representation
        {
            get { return (BytePresentation)GetValue(RepresentationProperty); }
            set { SetValue(RepresentationProperty, value); }
        }

        public static readonly DependencyProperty RepresentationProperty =
            DependencyProperty.Register(nameof(Representation), typeof(BytePresentation), typeof(ByteSizeToStringConverter), new PropertyMetadata(BytePresentation.Binary));

        public string OutOfRangeValue
        {
            get { return (string)GetValue(OutOfRangeValueProperty); }
            set { SetValue(OutOfRangeValueProperty, value); }
        }
        
        public static readonly DependencyProperty OutOfRangeValueProperty =
            DependencyProperty.Register("OutOfRangeValue", typeof(string), typeof(ByteSizeToStringConverter), new PropertyMetadata("???", OutOfRangeValuePropertyChangedCallback));

        private static void OutOfRangeValuePropertyChangedCallback(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
                throw new ArgumentNullException(nameof(OutOfRangeValue));
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            var size = System.Convert.ToDouble(value);
            if(size < 0)
                return OutOfRangeValue;
            string[] units;
            double powerBase;
            getUnits(out units, out powerBase);
            foreach(var unit in units)
            {
                if(size < 1000)
                {
                    return $"{size.ToString("0.000").Substring(0, 5)} {unit}";
                }
                size /= powerBase;
            }
            return OutOfRangeValue;
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            var sizeStr = value.ToString().Trim();
            string[] units;
            double powerBase;
            getUnits(out units, out powerBase);
            for(int i = 0; i < units.Length; i++)
            {
                if(sizeStr.EndsWith(units[i], StringComparison.OrdinalIgnoreCase))
                {
                    var sizeNumStr = sizeStr.Substring(0, sizeStr.Length - units[i].Length);
                    var sizeNum = double.Parse(sizeNumStr);
                    return (long)(sizeNum * Math.Pow(powerBase, i));
                }
            }
            throw new ArgumentException(nameof(value));
        }

        private void getUnits(out string[] units, out double powerBase)
        {
            if(Representation == BytePresentation.Metric)
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
    }

    public enum BytePresentation
    {
        Binary,
        Metric
    }
}
