using ExClient.Galleries;
using ExViewer.Settings;

namespace ExClient
{
    static class GalleryExtension
    {
        public static string GetDisplayTitle(this Gallery gallery)
        {
            if (gallery is null)
            {
                return "";
            }

            if (SettingCollection.Current.UseJapaneseTitle && !string.IsNullOrWhiteSpace(gallery.TitleJpn))
            {
                return gallery.TitleJpn;
            }
            else
            {
                return gallery.Title;
            }
        }

        public static string GetSecondaryTitle(this Gallery gallery)
        {
            if (gallery is null)
            {
                return "";
            }

            if (SettingCollection.Current.UseJapaneseTitle && !string.IsNullOrWhiteSpace(gallery.TitleJpn))
            {
                return gallery.Title;
            }
            else
            {
                return gallery.TitleJpn;
            }
        }
    }
}
