using System;
using Windows.UI.Xaml.Media;

namespace ExViewer.Helpers
{
    public static class HtmlHelper
    {
        public static string Color(Windows.UI.Color color)
        {
            if (color.A == 255)
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            return $"rgba({color.R},{color.G},{color.B},{color.A / 255d})";
        }

        public static string Color(SolidColorBrush brush)
            => Color(brush?.Color ?? throw new ArgumentNullException(nameof(brush)));
    }
}
