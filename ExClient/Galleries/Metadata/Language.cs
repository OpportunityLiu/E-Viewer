using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Galleries.Metadata
{
    public readonly struct Language : IEquatable<Language>
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
            if (tags is null)
                return default(Language);
            var modi = LanguageModifier.None;
            var language = new List<LanguageName>(1);
            var lanNA = false;
            foreach (var item in tags[Namespace.Language])
            {
                switch (item.Content.Content)
                {
                case "rewrite":
                    modi = LanguageModifier.Rewrite;
                    continue;
                case "translated":
                    modi = LanguageModifier.Translated;
                    continue;
                default:
                    if (naTags.Contains(item.Content.Content))
                    {
                        language.Clear();
                        lanNA = true;
                    }
                    else if (!lanNA)
                    {
                        if (Enum.TryParse<LanguageName>(item.Content.Content, true, out var lan))
                            language.Add(lan);
                        else
                            language.Add(LanguageName.Other);
                    }
                    continue;
                }
            }
            if (!lanNA && language.Count == 0)
                return new Language(null, modi);
            else
                return new Language(language, modi);
        }

        public Language(IEnumerable<LanguageName> names, LanguageModifier modifier)
        {
            Modifier = modifier;
            if (names is null)
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
            if (this.Names.Count == 0) // 0.9% cases
                return LocalizedStrings.Language.Names.NotApplicable;
            string name;
            if (this.Names.Count == 1) // 99% cases
                name = this.Names[0].ToFriendlyNameString();
            else // 0.1% cases
                name = string.Join(", ", this.Names.Select(LanguageNameExtension.ToFriendlyNameString));
            switch (Modifier)
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
            if (this.Modifier != other.Modifier)
                return false;
            if (this.names is null)
            {
                if (other.names is null)
                    return true;
                return false;
            }
            if (other.names is null)
                return false;
            return this.names.SequenceEqual(other.names);
        }

        public override bool Equals(object obj)
        {
            if (obj is Language l)
                return Equals(l);
            return false;
        }

        public override int GetHashCode()
        {
            var hash = this.Modifier.GetHashCode();
            foreach (var item in this.Names)
            {
                hash = unchecked(hash * 7 ^ item.GetHashCode());
            }
            return hash;
        }
    }
}