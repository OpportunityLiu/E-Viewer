using System;

namespace ExClient.Galleries.Metadata
{
    public static class LanguageNameExtension
    {
        public static string ToFriendlyNameString(this LanguageName that)
            => that.ToFriendlyNameString(LocalizedStrings.Language.Names.GetValue);
    }
}