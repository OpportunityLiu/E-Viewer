namespace ExClient.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Category
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("ExClient/Category");

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
        /// Cosplay
        /// </summary>
        public static string Cosplay => GetString("Cosplay");

        /// <summary>
        /// Doujinshi
        /// </summary>
        public static string Doujinshi => GetString("Doujinshi");

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
        /// Unspecified
        /// </summary>
        public static string Unspecified => GetString("Unspecified");

        /// <summary>
        /// Western
        /// </summary>
        public static string Western => GetString("Western");
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class NameSpace
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("ExClient/NameSpace");

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
        /// Artist
        /// </summary>
        public static string Artist => GetString("Artist");

        /// <summary>
        /// Character
        /// </summary>
        public static string Character => GetString("Character");

        /// <summary>
        /// Female
        /// </summary>
        public static string Female => GetString("Female");

        /// <summary>
        /// Group
        /// </summary>
        public static string Group => GetString("Group");

        /// <summary>
        /// Language
        /// </summary>
        public static string Language => GetString("Language");

        /// <summary>
        /// Male
        /// </summary>
        public static string Male => GetString("Male");

        /// <summary>
        /// Misc
        /// </summary>
        public static string Misc => GetString("Misc");

        /// <summary>
        /// Parody
        /// </summary>
        public static string Parody => GetString("Parody");

        /// <summary>
        /// Reclass
        /// </summary>
        public static string Reclass => GetString("Reclass");
    }

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
        /// The client has been disposed.
        /// </summary>
        public static string ClientDisposed => GetString("ClientDisposed");

        /// <summary>
        /// Gallery Title
        /// </summary>
        public static string DefaultTitle => GetString("DefaultTitle");

        /// <summary>
        /// Uploader
        /// </summary>
        public static string DefaultUploader => GetString("DefaultUploader");

        /// <summary>
        /// This torrent has beem expunged.
        /// </summary>
        public static string ExpungedTorrent => GetString("ExpungedTorrent");

        /// <summary>
        /// Only IPV4 address supported.
        /// </summary>
        public static string OnlyIpv4 => GetString("OnlyIpv4");

        /// <summary>
        /// User name or password incorrect.
        /// </summary>
        public static string WrongAccountInfo => GetString("WrongAccountInfo");

        /// <summary>
        /// The captcha was not entered correctly. Please try again.
        /// </summary>
        public static string WrongCaptcha => GetString("WrongCaptcha");
    }

}
