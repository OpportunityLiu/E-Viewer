using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient
{
    public enum LanguageModifier
    {
        None,
        Translated,
        Rewrite
    }

    public enum LanguageName : ushort
    {
        /// <summary>日语</summary>
        Japanese = 0,
        /// <summary>阿尔巴尼亚语</summary>
        Albanian,
        /// <summary>阿拉伯语</summary>
        Arabic,
        /// <summary>孟加拉语</summary>
        Bengali,
        /// <summary>加泰罗尼亚语</summary>
        Catalan,
        /// <summary>汉语</summary>
        Chinese,
        /// <summary>捷克语</summary>
        Czech,
        /// <summary>丹麦语</summary>
        Danish,
        /// <summary>荷兰语</summary>
        Dutch,
        /// <summary>英语</summary>
        English,
        /// <summary>世界语</summary>
        Esperanto,
        /// <summary>爱沙尼亚语</summary>
        Estonian,
        /// <summary>芬兰语</summary>
        Finnish,
        /// <summary>法语</summary>
        French,
        /// <summary>德语</summary>
        German,
        /// <summary>希腊语</summary>
        Greek,
        /// <summary>希伯来语</summary>
        Hebrew,
        /// <summary>印地语</summary>
        Hindi,
        /// <summary>匈牙利语</summary>
        Hungarian,
        /// <summary>印度尼西亚语</summary>
        Indonesian,
        /// <summary>意大利语</summary>
        Italian,
        /// <summary>朝鲜语</summary>
        Korean,
        /// <summary>蒙古语</summary>
        Mongolian,
        /// <summary>挪威语</summary>
        Norwegian,
        /// <summary>波兰语</summary>
        Polish,
        /// <summary>葡萄牙语</summary>
        Portuguese,
        /// <summary>罗马尼亚语</summary>
        Romanian,
        /// <summary>俄语</summary>
        Russian,
        /// <summary>斯洛伐克语</summary>
        Slovak,
        /// <summary>斯洛文尼亚语</summary>
        Slovenian,
        /// <summary>西班牙语</summary>
        Spanish,
        /// <summary>瑞典语</summary>
        Swedish,
        /// <summary>菲律宾语</summary>
        Tagalog,
        /// <summary>泰文语</summary>
        Thai,
        /// <summary>土耳其语</summary>
        Turkish,
        /// <summary>乌克兰语</summary>
        Ukrainian,
        /// <summary>越南语</summary>
        Vietnamese,
        Other = ushort.MaxValue
    }

    public static class LanguageNameExtension
    {
        public static string ToFriendlyNameString(this LanguageName that)
            => that.ToFriendlyNameString(LocalizedStrings.Language.Names.GetValue);
    }

    public struct Language : IEquatable<Language>
    {
        private static readonly string[] technicalTags = new string[]
        {
            "rewrite",
            "translated"
        };

        private static readonly string[] naTags = new string[]
        {
            "speechless",
            "text cleaned"
        };

        internal static Language Parse(Gallery gallery)
        {
            var tags = gallery.Tags;
            if(tags == null)
                return default(Language);
            var modi = LanguageModifier.None;
            var language = new List<LanguageName>(1);
            var lanNA = false;
            foreach(var item in tags[Namespace.Language])
            {
                switch(item.Content)
                {
                case "rewrite":
                    modi = LanguageModifier.Rewrite;
                    continue;
                case "translated":
                    modi = LanguageModifier.Translated;
                    continue;
                default:
                    if(naTags.Contains(item.Content))
                    {
                        language.Clear();
                        lanNA = true;
                    }
                    else if(!lanNA)
                    {
                        if(Enum.TryParse<LanguageName>(item.Content, true, out var lan))
                            language.Add(lan);
                        else
                            language.Add(LanguageName.Other);
                    }
                    continue;
                }
            }
            if(!lanNA && language.Count == 0)
                return new Language(null, modi);
            else
                return new Language(language, modi);
        }

        public Language(IEnumerable<LanguageName> names, LanguageModifier modifier)
        {
            Modifier = modifier;
            if(names == null)
            {
                this.names = null;
                return;
            }
            this.names = names.Distinct().ToArray();
        }

        private readonly LanguageName[] names;

        private static readonly IReadOnlyList<LanguageName> defaultLanguage = new[] { LanguageName.Japanese };

        public IReadOnlyList<LanguageName> Names => this.names ?? defaultLanguage;

        public LanguageModifier Modifier
        {
            get;
        }

        public override string ToString()
        {
            string name;
            if(this.Names.Count == 0)
                name = LocalizedStrings.Language.Names.NotApplicable;
            else if(this.Names.Count == 1)
                name = this.Names[0].ToFriendlyNameString();
            else
                name = string.Join(", ", this.Names.Select(LanguageNameExtension.ToFriendlyNameString));
            switch(Modifier)
            {
            case LanguageModifier.Translated:
                return $"{name} {LocalizedStrings.Language.Modifiers.Translated}";
            case LanguageModifier.Rewrite:
                return $"{name} {LocalizedStrings.Language.Modifiers.Rewrite}";
            default:
                return name;
            }
        }

        public bool Equals(Language other)
        {
            if(this.Modifier != other.Modifier)
                return false;
            if(this.names == null)
            {
                if(other.names == null)
                    return true;
                return false;
            }
            if(other.names == null)
                return false;
            return this.names.SequenceEqual(other.names);
        }

        public override bool Equals(object obj)
        {
            if(obj is Language l)
                return Equals(l);
            return false;
        }

        public override int GetHashCode()
        {
            var hash = this.Modifier.GetHashCode();
            foreach(var item in this.Names)
            {
                hash = unchecked(hash * 7 ^ item.GetHashCode());
            }
            return hash;
        }
    }
}