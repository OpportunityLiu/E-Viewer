namespace ExClient.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("ExClient/Resources");

        public static string GetString(string resourceKey)
        {
            string value;
            if(cache.TryGetValue(resourceKey, out value))
                return value;
            else
                return cache[resourceKey] = loader.GetString(resourceKey);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// This account dose not have permittion for exhentai.
        /// </summary>
        public static string AccountDenied => GetString("AccountDenied");

        /// <summary>
        /// All
        /// </summary>
        public static string All => GetString("All");

        /// <summary>
        /// Artist CG
        /// </summary>
        public static string ArtistCG => GetString("ArtistCG");

        /// <summary>
        /// Asian Porn
        /// </summary>
        public static string AsianPorn => GetString("AsianPorn");

        /// <summary>
        /// The client has been disposed.
        /// </summary>
        public static string ClientDisposed => GetString("ClientDisposed");

        /// <summary>
        /// Cosplay
        /// </summary>
        public static string Cosplay => GetString("Cosplay");

        /// <summary>
        /// Doujinshi
        /// </summary>
        public static string Doujinshi => GetString("Doujinshi");

        /// <summary>
        /// This torrent has beem expunged.
        /// </summary>
        public static string ExpungedTorrent => GetString("ExpungedTorrent");

        /// <summary>
        /// Game CG
        /// </summary>
        public static string GameCG => GetString("GameCG");

        /// <summary>
        /// Image Set
        /// </summary>
        public static string ImageSet => GetString("ImageSet");

        /// <summary>
        /// Manga
        /// </summary>
        public static string Manga => GetString("Manga");

        /// <summary>
        /// Misc
        /// </summary>
        public static string Misc => GetString("Misc");

        /// <summary>
        /// Non-H
        /// </summary>
        public static string NonH => GetString("NonH");

        /// <summary>
        /// Only IPV4 address supported.
        /// </summary>
        public static string OnlyIpv4 => GetString("OnlyIpv4");

        /// <summary>
        /// Unspecified
        /// </summary>
        public static string Unspecified => GetString("Unspecified");

        /// <summary>
        /// Western
        /// </summary>
        public static string Western => GetString("Western");

        /// <summary>
        /// The captcha was not entered correctly. Please try again.
        /// </summary>
        public static string WrongCaptcha => GetString("WrongCaptcha");
    }

}
