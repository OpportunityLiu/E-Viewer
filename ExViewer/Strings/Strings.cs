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
        /// Cancel
        /// </summary>
        public static string Cancel => GetString("Cancel");

        /// <summary>
        /// All saved galleries and cached galleries will be deleted.
        /// </summary>
        public static string ClearSavedDialogContent => GetString("ClearSavedDialogContent");

        /// <summary>
        /// ARE YOU SURE
        /// </summary>
        public static string ClearSavedDialogTitle => GetString("ClearSavedDialogTitle");

        /// <summary>
        /// Exit
        /// </summary>
        public static string Exit => GetString("Exit");

        /// <summary>
        /// Selected gallery has been deleted.
        /// </summary>
        public static string GalleryDeleted => GetString("GalleryDeleted");

        /// <summary>
        /// By {0}
        /// </summary>
        public static string GalleryPageCommentAuthorFormatString => GetString("GalleryPageCommentAuthorFormatString");

        /// <summary>
        /// Last edited on {0}
        /// </summary>
        public static string GalleryPageCommentEditedFormatString => GetString("GalleryPageCommentEditedFormatString");

        /// <summary>
        /// Posted on {0}
        /// </summary>
        public static string GalleryPageCommentPostedFormatString => GetString("GalleryPageCommentPostedFormatString");

        /// <summary>
        /// Score {0:+#0;-#0}
        /// </summary>
        public static string GalleryPageCommentScoreFormatString => GetString("GalleryPageCommentScoreFormatString");

        /// <summary>
        /// [{0}]
        /// </summary>
        public static string GalleryPagePivotHeaderNumberFormatString => GetString("GalleryPagePivotHeaderNumberFormatString");

        /// <summary>
        /// Start downloading...
        /// </summary>
        public static string GalleryPageTorrentDownloading => GetString("GalleryPageTorrentDownloading");

        /// <summary>
        /// Selected gallery has been saved to provided location.
        /// </summary>
        public static string GallerySavedTo => GetString("GallerySavedTo");

        /// <summary>
        /// {0} pages
        /// </summary>
        public static string GalleryViewerRecordCountFormatString => GetString("GalleryViewerRecordCountFormatString");

        /// <summary>
        /// Back to window
        /// </summary>
        public static string ImagePageBackToWindow => GetString("ImagePageBackToWindow");

        /// <summary>
        /// Full screen
        /// </summary>
        public static string ImagePageFullScreen => GetString("ImagePageFullScreen");

        /// <summary>
        /// File name: {0}
        /// Size: {1}
        /// Dimensions: {2} &#215; {3}
        /// </summary>
        public static string ImagePageImageFileInfo => GetString("ImagePageImageFileInfo");

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
        /// Please enter your password.
        /// </summary>
        public static string LogOnDialogNoPassword => GetString("LogOnDialogNoPassword");

        /// <summary>
        /// Please enter your user name.
        /// </summary>
        public static string LogOnDialogNoUserName => GetString("LogOnDialogNoUserName");

        /// <summary>
        /// OK
        /// </summary>
        public static string OK => GetString("OK");

        /// <summary>
        /// Can&#39;t parse the given string to a byte size.
        /// </summary>
        public static string ParseByteException => GetString("ParseByteException");

        /// <summary>
        /// Device is busy. Please try again later.
        /// </summary>
        public static string VerifyDeviceBusy => GetString("VerifyDeviceBusy");

        /// <summary>
        /// Because of your settings, we need to request the verification.
        /// </summary>
        public static string VerifyDialogContent => GetString("VerifyDialogContent");

        /// <summary>
        /// Verification has been disabled by group policy. Please contact your administrator.
        /// </summary>
        public static string VerifyDisabled => GetString("VerifyDisabled");

        /// <summary>
        /// VERIFICATION FAILED
        /// </summary>
        public static string VerifyFailedDialogTitle => GetString("VerifyFailedDialogTitle");

        /// <summary>
        /// Please set up a PIN first. 
        /// 
        /// Go &quot;Settings -&gt; Accounts - Sign-in options -&gt; PIN -&gt; Add&quot; to do this.
        /// </summary>
        public static string VerifyNotConfigured => GetString("VerifyNotConfigured");

        /// <summary>
        /// The verification has been canceled.
        /// </summary>
        public static string VerifyCanceled => GetString("VerifyCanceled");

        /// <summary>
        /// Plaese restart the app and try again.
        /// </summary>
        public static string VerifyOtherFailure => GetString("VerifyOtherFailure");

        /// <summary>
        /// You have exceeded the allowed number of verification. Plaese restart the app and try again.
        /// </summary>
        public static string VerifyRetriesExhausted => GetString("VerifyRetriesExhausted");

        /// <summary>
        /// The image hasn&#39;t finished loading.
        /// </summary>
        public static string ImagePageImageFileInfoDefault => GetString("ImagePageImageFileInfoDefault");

        /// <summary>
        /// All cached galleries will be deleted.
        /// </summary>
        public static string ClearCachedDialogContent => GetString("ClearCachedDialogContent");

        /// <summary>
        /// ARE YOU SURE
        /// </summary>
        public static string ClearCachedDialogTitle => GetString("ClearCachedDialogTitle");
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
        /// Disabled
        /// </summary>
        public static string BooleanDisabled => GetString("BooleanDisabled");

        /// <summary>
        /// Default title
        /// </summary>
        public static string BooleanDT => GetString("BooleanDT");

        /// <summary>
        /// Enabled
        /// </summary>
        public static string BooleanEnabled => GetString("BooleanEnabled");

        /// <summary>
        /// Japanese title (If available)
        /// </summary>
        public static string BooleanJT => GetString("BooleanJT");

        /// <summary>
        /// No
        /// </summary>
        public static string BooleanNo => GetString("BooleanNo");

        /// <summary>
        /// Off
        /// </summary>
        public static string BooleanOff => GetString("BooleanOff");

        /// <summary>
        /// On
        /// </summary>
        public static string BooleanOn => GetString("BooleanOn");

        /// <summary>
        /// Yes
        /// </summary>
        public static string BooleanYes => GetString("BooleanYes");

        /// <summary>
        /// The latency for the command bar to hide or show after tapping
        /// </summary>
        public static string ChangeCommandBarDelay => GetString("ChangeCommandBarDelay");

        /// <summary>
        /// CONNECTION
        /// </summary>
        public static string Connection => GetString("Connection");

        /// <summary>
        /// Zoom factor for double tapping
        /// </summary>
        public static string DefaultFactor => GetString("DefaultFactor");

        /// <summary>
        /// Default categories on the front page
        /// </summary>
        public static string DefaultSearchCategory => GetString("DefaultSearchCategory");

        /// <summary>
        /// Default keywords on the front page
        /// </summary>
        public static string DefaultSearchString => GetString("DefaultSearchString");

        /// <summary>
        /// GLOBAL
        /// </summary>
        public static string Global => GetString("Global");

        /// <summary>
        /// H@H
        /// </summary>
        public static string Hah => GetString("Hah");

        /// <summary>
        /// IP Address:Port (Leave blank to not use)
        /// </summary>
        public static string HahAddress => GetString("HahAddress");

        /// <summary>
        /// Passkey (Optional)
        /// </summary>
        public static string HahPasskey => GetString("HahPasskey");

        /// <summary>
        /// IMAGE VIEWING
        /// </summary>
        public static string ImageViewing => GetString("ImageViewing");

        /// <summary>
        /// Keep my screen on during image viewing
        /// </summary>
        public static string KeepScreenOn => GetString("KeepScreenOn");

        /// <summary>
        /// Always load compressed image
        /// </summary>
        public static string LoadLofiOnAllInternetConnection => GetString("LoadLofiOnAllInternetConnection");

        /// <summary>
        /// Load compressed image while using metered Internet connection
        /// </summary>
        public static string LoadLofiOnMeteredInternetConnection => GetString("LoadLofiOnMeteredInternetConnection");

        /// <summary>
        /// Maximum zoom factor
        /// </summary>
        public static string MaxFactor => GetString("MaxFactor");

        /// <summary>
        /// Inertia of mouse dragging
        /// </summary>
        public static string MouseInertial => GetString("MouseInertial");

        /// <summary>
        /// Verify my PIN when the app is starting
        /// </summary>
        public static string NeedVerify => GetString("NeedVerify");

        /// <summary>
        /// Save my lastest search as default
        /// </summary>
        public static string SaveLastSearch => GetString("SaveLastSearch");

        /// <summary>
        /// SEARCHING
        /// </summary>
        public static string Searching => GetString("Searching");

        /// <summary>
        /// The theme of the app
        /// </summary>
        public static string Theme => GetString("Theme");

        /// <summary>
        /// The default title displayed
        /// </summary>
        public static string UseJapaneseTitle => GetString("UseJapaneseTitle");

        /// <summary>
        /// Use Chinese translation of tags
        /// </summary>
        public static string UseChineseTagTranslation => GetString("UseChineseTagTranslation");

        /// <summary>
        /// Dark
        /// </summary>
        public static string ApplicationThemeDark => GetString("ApplicationThemeDark");

        /// <summary>
        /// Light
        /// </summary>
        public static string ApplicationThemeLight => GetString("ApplicationThemeLight");

        /// <summary>
        /// Left to right
        /// </summary>
        public static string BooleanLeftToRight => GetString("BooleanLeftToRight");

        /// <summary>
        /// Right to left
        /// </summary>
        public static string BooleanRightToLeft => GetString("BooleanRightToLeft");

        /// <summary>
        /// Flip direction
        /// </summary>
        public static string ReverseFlowDirection => GetString("ReverseFlowDirection");

        /// <summary>
        /// Use Japanese translation of tags
        /// </summary>
        public static string UseJapaneseTagTranslation => GetString("UseJapaneseTagTranslation");

        /// <summary>
        /// Excluded languages
        /// </summary>
        public static string ExcludedLanguages => GetString("ExcludedLanguages");

        /// <summary>
        /// Chinese
        /// </summary>
        public static string Chinese => GetString("Chinese");

        /// <summary>
        /// Dutch
        /// </summary>
        public static string Dutch => GetString("Dutch");

        /// <summary>
        /// English
        /// </summary>
        public static string English => GetString("English");

        /// <summary>
        /// French
        /// </summary>
        public static string French => GetString("French");

        /// <summary>
        /// German
        /// </summary>
        public static string German => GetString("German");

        /// <summary>
        /// Hungarian
        /// </summary>
        public static string Hungarian => GetString("Hungarian");

        /// <summary>
        /// Italian
        /// </summary>
        public static string Italian => GetString("Italian");

        /// <summary>
        /// Japanese
        /// </summary>
        public static string Japanese => GetString("Japanese");

        /// <summary>
        /// Korean
        /// </summary>
        public static string Korean => GetString("Korean");

        /// <summary>
        /// N/A
        /// </summary>
        public static string NotApplicable => GetString("NotApplicable");

        /// <summary>
        /// Other
        /// </summary>
        public static string Other => GetString("Other");

        /// <summary>
        /// Polish
        /// </summary>
        public static string Polish => GetString("Polish");

        /// <summary>
        /// Portuguese
        /// </summary>
        public static string Portuguese => GetString("Portuguese");

        /// <summary>
        /// Russian
        /// </summary>
        public static string Russian => GetString("Russian");

        /// <summary>
        /// Spanish
        /// </summary>
        public static string Spanish => GetString("Spanish");

        /// <summary>
        /// Thai
        /// </summary>
        public static string Thai => GetString("Thai");

        /// <summary>
        /// Vietnamese
        /// </summary>
        public static string Vietnamese => GetString("Vietnamese");
    }

}
