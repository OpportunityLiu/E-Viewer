using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ExClient
{
    static class FavoriteExtension
    {
        private static readonly ResourceDictionary favoritesBrushes = getResource();

        private static ResourceDictionary getResource()
        {
            var r = new ResourceDictionary();
            Application.LoadComponent(r, new Uri("ms-appx:///Themes/Favorites.xaml"));
            return r;
        }

        public static Brush GetThemeBrush(this FavoriteCategory cat)
        {
            var idx = cat?.Index ?? -1;
            if (idx < 0)
                return (Brush)favoritesBrushes["FavoriteCategoryNone"];
            return (Brush)favoritesBrushes[$"FavoriteCategory{cat.Index}"];
        }
    }
}