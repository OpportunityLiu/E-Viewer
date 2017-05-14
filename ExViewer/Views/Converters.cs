using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using ExViewer.Settings;
using ExViewer.ViewModels;
using Windows.UI;
using System.Globalization;
using Windows.UI.Xaml.Markup;
using ExViewer.Converters;
using Opportunity.Converters;
using ExClient.Galleries;
using ExClient.Tagging;

namespace ExViewer.Views
{
    public class LoadStateToVisualStateConverter : ValueConverter
    { 
        private static Brush accent;

        public static Brush AccentBrush =>
            System.Threading.LazyInitializer.EnsureInitialized(ref accent, () => (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"]);

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (ImageLoadingState)value;
            if(targetType == typeof(Visibility))
            {
                if(state == ImageLoadingState.Loaded)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            if(targetType == typeof(Brush))
            {
                if(state == ImageLoadingState.Failed)
                    return new SolidColorBrush(Colors.Red);
                else
                    return AccentBrush;
            }
            if(targetType == typeof(bool))
            {
                if(state == ImageLoadingState.Waiting || state == ImageLoadingState.Preparing)
                    return true;
                else
                    return false;
            }
            throw new NotImplementedException();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class RateStringConverter : ValueConverter
    {
        const char halfL = '\xE7C6';
        const char full = '\xE1CF';

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var rating = ((double)value) * 2;
            var x = (int)Math.Round(rating);
            var fullCount = x / 2;
            var halfCount = x - 2 * fullCount;
            return new string(full, fullCount) + new string(halfL, halfCount);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class GalleryToTitleStringConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var g = value as Gallery;
            if(g == null)
                return "";
            return g.GetDisplayTitle();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OperationStateToBrushConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (OperationState)value;
            switch(v)
            {
            case OperationState.NotStarted:
                return new SolidColorBrush(Colors.Transparent);
            case OperationState.Started:
                return (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
            case OperationState.Failed:
                return new SolidColorBrush(Colors.Red);
            case OperationState.Completed:
                return new SolidColorBrush(Colors.Green);
            }
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NamespaceToFriendlyStringConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((Namespace)value).ToFriendlyNameString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
