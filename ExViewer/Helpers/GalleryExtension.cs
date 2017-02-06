using ExViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    static class GalleryExtension
    {
        public static string GetDisplayTitle(this Gallery gallery)
        {
            if(SettingCollection.Current.UseJapaneseTitle && !string.IsNullOrWhiteSpace(gallery.TitleJpn))
                return gallery.TitleJpn;
            else
                return gallery.Title;
        }

        public static string GetSecondaryTitle(this Gallery gallery)
        {
            if(SettingCollection.Current.UseJapaneseTitle && !string.IsNullOrWhiteSpace(gallery.TitleJpn))
                return gallery.Title;
            else
                return gallery.TitleJpn;
        }
    }
}
