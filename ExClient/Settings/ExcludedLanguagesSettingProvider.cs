using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Settings
{
    public enum ExcludedLanguage : ushort
    {
        /// <summary>
        /// Do not use.
        /// </summary>
        [Obsolete("Do not use this enum, JapaneseOriginal can't be an excluded language.")]
        JapaneseOriginal=0,   JapaneseTranslated = 0 + 1024, JapaneseRewrite = 0 + 2048,
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

    public sealed class ExcludedLanguagesSettingProvider : SettingProvider, ICollection<ExcludedLanguage>
    {
        internal ExcludedLanguagesSettingProvider()
        {
        }

        public static string ToString(IEnumerable<ExcludedLanguage> items)
        {
            return string.Join(", ", items);
        }

        public static IEnumerable<ExcludedLanguage> FromString(string value)
        {
            foreach(var item in (value ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                ExcludedLanguage r;
                if(Enum.TryParse(item, out r))
                    yield return r;
            }
        }

        internal override string GetCookieContent()
        {
            if(items.Count == 0)
                return null;
            return $"xl_{string.Join("x", items)}";
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public void AddRange(IEnumerable<ExcludedLanguage> items)
        {
            foreach(var item in items)
            {
                if(!Enum.IsDefined(typeof(ExcludedLanguage), item))
                    throw new ArgumentOutOfRangeException(nameof(item));
                this.items.Add((ushort)item);
            }
            ApplyChanges();
        }

        private HashSet<ushort> items = new HashSet<ushort>();

        public void Add(ExcludedLanguage item)
        {
            if(!Enum.IsDefined(typeof(ExcludedLanguage), item))
                throw new ArgumentOutOfRangeException(nameof(item));
            if(items.Add((ushort)item))
                ApplyChanges();
        }

        public void Clear()
        {
            if(items.Count == 0)
                return;
            items.Clear();
            ApplyChanges();
        }

        public bool Contains(ExcludedLanguage item) => items.Contains((ushort)item);

        public void CopyTo(ExcludedLanguage[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach(var item in this)
            {
                if(i >= array.Length)
                    break;
                array[i] = item;
                i++;
            }
        }

        public bool Remove(ExcludedLanguage item)
        {
            var r = items.Remove((ushort)item);
            if(r)
                ApplyChanges();
            return r;
        }

        public IEnumerator<ExcludedLanguage> GetEnumerator() => items.Cast<ExcludedLanguage>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public int Count => items.Count;

        public bool IsReadOnly => false;
    }
}
