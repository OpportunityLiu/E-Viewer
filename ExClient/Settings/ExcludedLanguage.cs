using ExClient.Galleries.Metadata;

namespace ExClient.Settings
{
    public enum ExcludedLanguage : ushort
    {
        JapaneseOriginal = 0, JapaneseTranslated = 0 + 1024, JapaneseRewrite = 0 + 2048,
        EnglishOriginal = 1, EnglishTranslated = EnglishOriginal + 1024, EnglishRewrite = EnglishOriginal + 2048,
        ChineseOriginal = 10, ChineseTranslated = ChineseOriginal + 1024, ChineseRewrite = ChineseOriginal + 2048,
        DutchOriginal = 20, DutchTranslated = DutchOriginal + 1024, DutchRewrite = DutchOriginal + 2048,
        FrenchOriginal = 30, FrenchTranslated = FrenchOriginal + 1024, FrenchRewrite = FrenchOriginal + 2048,
        GermanOriginal = 40, GermanTranslated = GermanOriginal + 1024, GermanRewrite = GermanOriginal + 2048,
        HungarianOriginal = 50, HungarianTranslated = HungarianOriginal + 1024, HungarianRewrite = HungarianOriginal + 2048,
        ItalianOriginal = 60, ItalianTranslated = ItalianOriginal + 1024, ItalianRewrite = ItalianOriginal + 2048,
        KoreanOriginal = 70, KoreanTranslated = KoreanOriginal + 1024, KoreanRewrite = KoreanOriginal + 2048,
        PolishOriginal = 80, PolishTranslated = PolishOriginal + 1024, PolishRewrite = PolishOriginal + 2048,
        PortugueseOriginal = 90, PortugueseTranslated = PortugueseOriginal + 1024, PortugueseRewrite = PortugueseOriginal + 2048,
        RussianOriginal = 100, RussianTranslated = RussianOriginal + 1024, RussianRewrite = RussianOriginal + 2048,
        SpanishOriginal = 110, SpanishTranslated = SpanishOriginal + 1024, SpanishRewrite = SpanishOriginal + 2048,
        ThaiOriginal = 120, ThaiTranslated = ThaiOriginal + 1024, ThaiRewrite = ThaiOriginal + 2048,
        VietnameseOriginal = 130, VietnameseTranslated = VietnameseOriginal + 1024, VietnameseRewrite = VietnameseOriginal + 2048,
        NotApplicableOriginal = 254, NotApplicableTranslated = NotApplicableOriginal + 1024, NotApplicableRewrite = NotApplicableOriginal + 2048,
        OtherOriginal = 255, OtherTranslated = OtherOriginal + 1024, OtherRewrite = OtherOriginal + 2048,
    }
}
