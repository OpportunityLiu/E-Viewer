namespace ExClient.ExClient_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IResourceProvider
    {
        global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider this[string resourceKey] { get; }
        string GetValue(string resourceKey);
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IGeneratedResourceProvider : global::ExClient.ExClient_ResourceInfo.IResourceProvider
    {
        string Value { get; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    [System.Diagnostics.DebuggerDisplay("\\{{Value}\\}")]
    internal struct GeneratedResourceProvider : global::ExClient.ExClient_ResourceInfo.IGeneratedResourceProvider
    {
        internal GeneratedResourceProvider(string key)
        {
            this.key = key;
        }

        private readonly string key;

        public string Value => global::ExClient.LocalizedStrings.GetValue(key);

        public GeneratedResourceProvider this[string resourceKey]
        {
            get
            {
                if(resourceKey == null)
                    throw new global::System.ArgumentNullException();
                return new global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider($"{key}/{resourceKey}");
            }
        }

        public string GetValue(string resourceKey)
        {
            if(resourceKey == null)
                return this.Value;
            return global::ExClient.LocalizedStrings.GetValue($"{key}/{resourceKey}");
        }
    }
}

namespace ExClient.ExClient_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface ICategory : global::ExClient.ExClient_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>All</para>
        /// </summary>
        string All { get; }
        /// <summary>
        /// <para>Artist CG</para>
        /// </summary>
        string ArtistCG { get; }
        /// <summary>
        /// <para>Asian Porn</para>
        /// </summary>
        string AsianPorn { get; }
        /// <summary>
        /// <para>Cosplay</para>
        /// </summary>
        string Cosplay { get; }
        /// <summary>
        /// <para>Doujinshi</para>
        /// </summary>
        string Doujinshi { get; }
        /// <summary>
        /// <para>Game CG</para>
        /// </summary>
        string GameCG { get; }
        /// <summary>
        /// <para>Image Set</para>
        /// </summary>
        string ImageSet { get; }
        /// <summary>
        /// <para>Manga</para>
        /// </summary>
        string Manga { get; }
        /// <summary>
        /// <para>Misc</para>
        /// </summary>
        string Misc { get; }
        /// <summary>
        /// <para>Non-H</para>
        /// </summary>
        string NonH { get; }
        /// <summary>
        /// <para>Unspecified</para>
        /// </summary>
        string Unspecified { get; }
        /// <summary>
        /// <para>Western</para>
        /// </summary>
        string Western { get; }
    }
}

namespace ExClient.ExClient_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface INamespace : global::ExClient.ExClient_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>Artist</para>
        /// </summary>
        string Artist { get; }
        /// <summary>
        /// <para>Character</para>
        /// </summary>
        string Character { get; }
        /// <summary>
        /// <para>Female</para>
        /// </summary>
        string Female { get; }
        /// <summary>
        /// <para>Group</para>
        /// </summary>
        string Group { get; }
        /// <summary>
        /// <para>Language</para>
        /// </summary>
        string Language { get; }
        /// <summary>
        /// <para>Male</para>
        /// </summary>
        string Male { get; }
        /// <summary>
        /// <para>Misc</para>
        /// </summary>
        string Misc { get; }
        /// <summary>
        /// <para>Parody</para>
        /// </summary>
        string Parody { get; }
        /// <summary>
        /// <para>Reclass</para>
        /// </summary>
        string Reclass { get; }
        /// <summary>
        /// <para>Unknown</para>
        /// </summary>
        string Unknown { get; }
    }
}

namespace ExClient.ExClient_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IResources : global::ExClient.ExClient_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>This account dose not have permittion for exhentai.</para>
        /// </summary>
        string NoExPermittion { get; }
        /// <summary>
        /// <para>All favorites</para>
        /// </summary>
        string AllFavorites { get; }
        /// <summary>
        /// <para>Remove from favorites</para>
        /// </summary>
        string RemoveFromFavorites { get; }
        /// <summary>
        /// <para>The client has been disposed.</para>
        /// </summary>
        string ClientDisposed { get; }
        /// <summary>
        /// <para>Gallery Title</para>
        /// </summary>
        string DefaultTitle { get; }
        /// <summary>
        /// <para>Uploader</para>
        /// </summary>
        string DefaultUploader { get; }
        /// <summary>
        /// <para>This torrent has beem expunged.</para>
        /// </summary>
        string ExpungedTorrent { get; }
        /// <summary>
        /// <para>Only IPV4 address supported.</para>
        /// </summary>
        string OnlyIpv4 { get; }
        /// <summary>
        /// <para>User name or password incorrect.</para>
        /// </summary>
        string WrongAccountInfo { get; }
        /// <summary>
        /// <para>The captcha was not entered correctly. Please try again.</para>
        /// </summary>
        string WrongCaptcha { get; }
        /// <summary>
        /// <para>The response of the server can&#39;t be analyzed.</para>
        /// </summary>
        string WrongVoteResponse { get; }
        /// <summary>
        /// <para>Can&#39;t vote when you had posted a comment.</para>
        /// </summary>
        string WrongVoteState { get; }
        /// <summary>
        /// <para>Can only edit own comment.</para>
        /// </summary>
        string WrongEditState { get; }
        /// <summary>
        /// <para>You did not enter a valid comment.</para>
        /// </summary>
        string EmptyComment { get; }
        /// <summary>
        /// <para>Your comment is too short.</para>
        /// </summary>
        string ShortComment { get; }
        /// <summary>
        /// <para>You can only add comments for active galleries.</para>
        /// </summary>
        string WrongGalleryState { get; }
    }
}

namespace ExClient
{
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal static class LocalizedStrings
    {
        private static global::System.Collections.Generic.IDictionary<string, string> __cache____HmLypAgE;
        private static global::Windows.ApplicationModel.Resources.ResourceLoader __loader____yHIjpYPV;

        static LocalizedStrings()
        {
            global::ExClient.LocalizedStrings.__cache____HmLypAgE = new global::System.Collections.Generic.Dictionary<string, string>();
            global::ExClient.LocalizedStrings.__loader____yHIjpYPV = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        }

        public static string GetValue(string resourceKey)
        {
            string value;
            if(global::ExClient.LocalizedStrings.__cache____HmLypAgE.TryGetValue(resourceKey, out value))
                return value;
            return global::ExClient.LocalizedStrings.__cache____HmLypAgE[resourceKey] = global::ExClient.LocalizedStrings.__loader____yHIjpYPV.GetString(resourceKey);
        }


        internal static global::ExClient.ExClient_ResourceInfo.ICategory Category { get; } = new global::ExClient.LocalizedStrings.Category__8nrOCeVP();

        [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\\}")]
        private sealed class Category__8nrOCeVP : global::ExClient.ExClient_ResourceInfo.ICategory
        {
            global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider global::ExClient.ExClient_ResourceInfo.IResourceProvider.this[string resourceKey]
            {
                get
                {
                    if(resourceKey == null)
                        throw new global::System.ArgumentNullException();
                    return new global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory/" + resourceKey);
                }
            }

            string global::ExClient.ExClient_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
            {
                return global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory/" + resourceKey);
            }


            string global::ExClient.ExClient_ResourceInfo.ICategory.All
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FAll");
            string global::ExClient.ExClient_ResourceInfo.ICategory.ArtistCG
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FArtistCG");
            string global::ExClient.ExClient_ResourceInfo.ICategory.AsianPorn
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FAsianPorn");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Cosplay
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FCosplay");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Doujinshi
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FDoujinshi");
            string global::ExClient.ExClient_ResourceInfo.ICategory.GameCG
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FGameCG");
            string global::ExClient.ExClient_ResourceInfo.ICategory.ImageSet
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FImageSet");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Manga
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FManga");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Misc
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FMisc");
            string global::ExClient.ExClient_ResourceInfo.ICategory.NonH
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FNonH");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Unspecified
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FUnspecified");
            string global::ExClient.ExClient_ResourceInfo.ICategory.Western
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FCategory\u002FWestern");
        }

        internal static global::ExClient.ExClient_ResourceInfo.INamespace Namespace { get; } = new global::ExClient.LocalizedStrings.Namespace__IVGb5HLZ();

        [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\\}")]
        private sealed class Namespace__IVGb5HLZ : global::ExClient.ExClient_ResourceInfo.INamespace
        {
            global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider global::ExClient.ExClient_ResourceInfo.IResourceProvider.this[string resourceKey]
            {
                get
                {
                    if(resourceKey == null)
                        throw new global::System.ArgumentNullException();
                    return new global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace/" + resourceKey);
                }
            }

            string global::ExClient.ExClient_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
            {
                return global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace/" + resourceKey);
            }


            string global::ExClient.ExClient_ResourceInfo.INamespace.Artist
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FArtist");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Character
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FCharacter");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Female
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FFemale");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Group
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FGroup");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Language
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FLanguage");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Male
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FMale");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Misc
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FMisc");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Parody
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FParody");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Reclass
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FReclass");
            string global::ExClient.ExClient_ResourceInfo.INamespace.Unknown
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FNamespace\u002FUnknown");
        }

        internal static global::ExClient.ExClient_ResourceInfo.IResources Resources { get; } = new global::ExClient.LocalizedStrings.Resources__yZH7w3jW();

        [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\\}")]
        private sealed class Resources__yZH7w3jW : global::ExClient.ExClient_ResourceInfo.IResources
        {
            global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider global::ExClient.ExClient_ResourceInfo.IResourceProvider.this[string resourceKey]
            {
                get
                {
                    if(resourceKey == null)
                        throw new global::System.ArgumentNullException();
                    return new global::ExClient.ExClient_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources/" + resourceKey);
                }
            }

            string global::ExClient.ExClient_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
            {
                return global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources/" + resourceKey);
            }


            string global::ExClient.ExClient_ResourceInfo.IResources.NoExPermittion
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FNoExPermittion");
            string global::ExClient.ExClient_ResourceInfo.IResources.AllFavorites
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FAllFavorites");
            string global::ExClient.ExClient_ResourceInfo.IResources.RemoveFromFavorites
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FRemoveFromFavorites");
            string global::ExClient.ExClient_ResourceInfo.IResources.ClientDisposed
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FClientDisposed");
            string global::ExClient.ExClient_ResourceInfo.IResources.DefaultTitle
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FDefaultTitle");
            string global::ExClient.ExClient_ResourceInfo.IResources.DefaultUploader
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FDefaultUploader");
            string global::ExClient.ExClient_ResourceInfo.IResources.ExpungedTorrent
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FExpungedTorrent");
            string global::ExClient.ExClient_ResourceInfo.IResources.OnlyIpv4
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FOnlyIpv4");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongAccountInfo
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongAccountInfo");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongCaptcha
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongCaptcha");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongVoteResponse
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongVoteResponse");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongVoteState
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongVoteState");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongEditState
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongEditState");
            string global::ExClient.ExClient_ResourceInfo.IResources.EmptyComment
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FEmptyComment");
            string global::ExClient.ExClient_ResourceInfo.IResources.ShortComment
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FShortComment");
            string global::ExClient.ExClient_ResourceInfo.IResources.WrongGalleryState
                => global::ExClient.LocalizedStrings.GetValue("ms-resource\u003A\u002F\u002F\u002FExClient\u002FResources\u002FWrongGalleryState");
        }
    }
}
