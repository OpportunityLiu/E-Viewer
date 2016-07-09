namespace ExViewer.LocalizedStrings
{    
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Resources
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("/Resources");

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
        /// E-Viewer is a client for e-hentai.org and exhentai.org.
        /// </summary>
        public static string AppDescription => GetString("AppDescription");

        /// <summary>
        /// E-Viewer
        /// </summary>
        public static string AppDisplayName => GetString("AppDisplayName");

        /// <summary>
        /// The text associated with this error code could not be found.
        /// </summary>
        public static string ErrorPrefix => GetString("ErrorPrefix");
    }

}
