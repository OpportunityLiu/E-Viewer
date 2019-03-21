namespace ExClient.Services
{
    public static class LanguageModifierExtension
    {
        public static string ToFriendlyNameString(this LanguageModifier that)
        {
            switch (that)
            {
            case LanguageModifier.Translated:
                return LocalizedStrings.Language.Modifiers.Translated;
            case LanguageModifier.Rewrite:
                return LocalizedStrings.Language.Modifiers.Rewrite;
            default:
                return "";
            }
        }
    }

    public enum LanguageModifier
    {
        None,
        Translated,
        Rewrite
    }
}