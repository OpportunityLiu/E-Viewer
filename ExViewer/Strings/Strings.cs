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
        /// Back to window
        /// </summary>
        public static string BackToWindow => GetString("BackToWindow");

        /// <summary>
        /// Cancel
        /// </summary>
        public static string Cancel => GetString("Cancel");

        /// <summary>
        /// All saved galleries will be deleted.
        /// </summary>
        public static string ClearCacheDialogContent => GetString("ClearCacheDialogContent");

        /// <summary>
        /// ARE YOU SURE
        /// </summary>
        public static string ClearCacheDialogTitle => GetString("ClearCacheDialogTitle");

        /// <summary>
        /// Exit
        /// </summary>
        public static string Exit => GetString("Exit");

        /// <summary>
        /// Full screen
        /// </summary>
        public static string FullScreen => GetString("FullScreen");

        /// <summary>
        /// Selected gallery has been deleted.
        /// </summary>
        public static string GalleryDeleted => GetString("GalleryDeleted");

        /// <summary>
        /// Selected gallery has been saved to provided location.
        /// </summary>
        public static string GallerySavedTo => GetString("GallerySavedTo");

        /// <summary>
        /// File name: {0}
        /// Size: {1}
        /// Dimensions: {2} × {3}
        /// </summary>
        public static string ImageFileInfo => GetString("ImageFileInfo");

        /// <summary>
        /// Zoom in/out			SPACE
        /// Zoom to maximum		=
        /// Zoom to minimun			-
        /// Zoom in				]
        /// Zoom out			[
        /// Pan / Next/Previous Image		ARROW KEYS
        /// Full screen			ENTER
        /// </summary>
        public static string ImageViewTipsContent => GetString("ImageViewTipsContent");

        /// <summary>
        /// TIPS FOR KEYBOARD USERS
        /// </summary>
        public static string ImageViewTipsTitle => GetString("ImageViewTipsTitle");

        /// <summary>
        /// Because of your settings, we need to request the verification.
        /// </summary>
        public static string NeedVerify => GetString("NeedVerify");

        /// <summary>
        /// Please enter your password.
        /// </summary>
        public static string NoPassword => GetString("NoPassword");

        /// <summary>
        /// Please enter your user name.
        /// </summary>
        public static string NoUserName => GetString("NoUserName");

        /// <summary>
        /// OK
        /// </summary>
        public static string OK => GetString("OK");

        /// <summary>
        /// Can't parse the given string to a byte size.
        /// </summary>
        public static string ParseByteException => GetString("ParseByteException");

        /// <summary>
        /// Start downloading...
        /// </summary>
        public static string TorrentDownloading => GetString("TorrentDownloading");

        /// <summary>
        /// Device is busy. Please try again later.
        /// </summary>
        public static string VerifyDeviceBusy => GetString("VerifyDeviceBusy");

        /// <summary>
        /// Verification has been disabled by group policy. Please contact your administrator.
        /// </summary>
        public static string VerifyDisabled => GetString("VerifyDisabled");

        /// <summary>
        /// Verify failed. Plaese try again.
        /// </summary>
        public static string VerifyFailed => GetString("VerifyFailed");

        /// <summary>
        /// VERIFICATION FAILED
        /// </summary>
        public static string VerifyFailedDialogTitle => GetString("VerifyFailedDialogTitle");

        /// <summary>
        /// Please set up a PIN first. 
        /// 
        /// Go &amp;quot;Settings -&gt; Accounts - Sign-in options -&gt; PIN -&gt; Add&amp;quot; to do this.
        /// </summary>
        public static string VerifyNotConfigured => GetString("VerifyNotConfigured");

        /// <summary>
        /// The captcha was not entered correctly. Please try again.
        /// </summary>
        public static string WrongCaptcha => GetString("WrongCaptcha");
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ResourceGenerator","1.0")]
    public static class Settings
    {
        private static readonly global::System.Collections.Generic.Dictionary<string, string> cache 
            = new global::System.Collections.Generic.Dictionary<string, string>();

        private static readonly global::Windows.ApplicationModel.Resources.ResourceLoader loader 
            = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("/Settings");

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
        /// Off
        /// </summary>
        public static string BooleanOff => GetString("BooleanOff");

        /// <summary>
        /// On
        /// </summary>
        public static string BooleanOn => GetString("BooleanOn");

        /// <summary>
        /// Searching
        /// </summary>
        public static string Searching => GetString("Searching");
    }

}
