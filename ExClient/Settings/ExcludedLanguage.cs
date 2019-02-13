using System;
using static ExClient.Settings.ExcludedLanguageExtension;

namespace ExClient.Settings
{
    public enum ExcludedLanguage
    {
        Default = 0,
        [Obsolete("Not supported by server")]
        JapaneseOriginal = 0, JapaneseTranslated = 0 + Translated, JapaneseRewrite = 0 + Rewrite,
        EnglishOriginal = 1, EnglishTranslated = EnglishOriginal + Translated, EnglishRewrite = EnglishOriginal + Rewrite,
        ChineseOriginal = 10, ChineseTranslated = ChineseOriginal + Translated, ChineseRewrite = ChineseOriginal + Rewrite,
        DutchOriginal = 20, DutchTranslated = DutchOriginal + Translated, DutchRewrite = DutchOriginal + Rewrite,
        FrenchOriginal = 30, FrenchTranslated = FrenchOriginal + Translated, FrenchRewrite = FrenchOriginal + Rewrite,
        GermanOriginal = 40, GermanTranslated = GermanOriginal + Translated, GermanRewrite = GermanOriginal + Rewrite,
        HungarianOriginal = 50, HungarianTranslated = HungarianOriginal + Translated, HungarianRewrite = HungarianOriginal + Rewrite,
        ItalianOriginal = 60, ItalianTranslated = ItalianOriginal + Translated, ItalianRewrite = ItalianOriginal + Rewrite,
        KoreanOriginal = 70, KoreanTranslated = KoreanOriginal + Translated, KoreanRewrite = KoreanOriginal + Rewrite,
        PolishOriginal = 80, PolishTranslated = PolishOriginal + Translated, PolishRewrite = PolishOriginal + Rewrite,
        PortugueseOriginal = 90, PortugueseTranslated = PortugueseOriginal + Translated, PortugueseRewrite = PortugueseOriginal + Rewrite,
        RussianOriginal = 100, RussianTranslated = RussianOriginal + Translated, RussianRewrite = RussianOriginal + Rewrite,
        SpanishOriginal = 110, SpanishTranslated = SpanishOriginal + Translated, SpanishRewrite = SpanishOriginal + Rewrite,
        ThaiOriginal = 120, ThaiTranslated = ThaiOriginal + Translated, ThaiRewrite = ThaiOriginal + Rewrite,
        VietnameseOriginal = 130, VietnameseTranslated = VietnameseOriginal + Translated, VietnameseRewrite = VietnameseOriginal + Rewrite,
        NotApplicableOriginal = 254, NotApplicableTranslated = NotApplicableOriginal + Translated, NotApplicableRewrite = NotApplicableOriginal + Rewrite,
        OtherOriginal = 255, OtherTranslated = OtherOriginal + Translated, OtherRewrite = OtherOriginal + Rewrite,
    }

    public static class ExcludedLanguageExtension
    {
        internal const ExcludedLanguage Translated = (ExcludedLanguage)1024;
        internal const ExcludedLanguage Rewrite = (ExcludedLanguage)2048;

        public static string ToDisplayNameString(this ExcludedLanguage language)
        {
            var languages = LocalizedStrings.Language.Names;
            var modifiers = LocalizedStrings.Language.Modifiers;

            var modifier = default(string);
            if (language.IsTranslated())
            {
                modifier = modifiers.Translated;
                language -= Translated;
            }
            else if (language.IsRewrite())
            {
                modifier = modifiers.Rewrite;
                language -= Rewrite;
            }

            string lang;
            switch (language)
            {
            case ExcludedLanguage.Default:
                lang = LocalizedStrings.Language.Names.Japanese;
                break;
            case ExcludedLanguage.EnglishOriginal:
                lang = LocalizedStrings.Language.Names.English;
                break;
            case ExcludedLanguage.ChineseOriginal:
                lang = LocalizedStrings.Language.Names.Chinese;
                break;
            case ExcludedLanguage.DutchOriginal:
                lang = LocalizedStrings.Language.Names.Dutch;
                break;
            case ExcludedLanguage.FrenchOriginal:
                lang = LocalizedStrings.Language.Names.French;
                break;
            case ExcludedLanguage.GermanOriginal:
                lang = LocalizedStrings.Language.Names.German;
                break;
            case ExcludedLanguage.HungarianOriginal:
                lang = LocalizedStrings.Language.Names.Hungarian;
                break;
            case ExcludedLanguage.ItalianOriginal:
                lang = LocalizedStrings.Language.Names.Italian;
                break;
            case ExcludedLanguage.KoreanOriginal:
                lang = LocalizedStrings.Language.Names.Korean;
                break;
            case ExcludedLanguage.PolishOriginal:
                lang = LocalizedStrings.Language.Names.Polish;
                break;
            case ExcludedLanguage.PortugueseOriginal:
                lang = LocalizedStrings.Language.Names.Portuguese;
                break;
            case ExcludedLanguage.RussianOriginal:
                lang = LocalizedStrings.Language.Names.Russian;
                break;
            case ExcludedLanguage.SpanishOriginal:
                lang = LocalizedStrings.Language.Names.Spanish;
                break;
            case ExcludedLanguage.ThaiOriginal:
                lang = LocalizedStrings.Language.Names.Thai;
                break;
            case ExcludedLanguage.VietnameseOriginal:
                lang = LocalizedStrings.Language.Names.Vietnamese;
                break;
            case ExcludedLanguage.NotApplicableOriginal:
                lang = LocalizedStrings.Language.Names.NotApplicable;
                break;
            default:
                lang = LocalizedStrings.Language.Names.Other;
                break;
            }

            if (modifier is null)
                return lang;
            return lang + " " + modifier;
        }

        public static bool IsOriginal(this ExcludedLanguage language) => language < Translated;

        public static bool IsTranslated(this ExcludedLanguage language) => language < Rewrite && language >= Translated;

        public static bool IsRewrite(this ExcludedLanguage language) => language >= Rewrite;
    }
}
