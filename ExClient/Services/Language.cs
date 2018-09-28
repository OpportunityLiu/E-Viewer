using ExClient.Galleries;
using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Services
{
    public static class LanguageExtension
    {
        private static readonly string[] technicalTags = new[]
        {
            "rewrite",
            "translated"
        };

        private static readonly string[] naTags = new[]
        {
            "speechless",
            "text cleaned"
        };

        public static Language GetLanguage(this Gallery gallery)
        {
            var tags = gallery?.Tags;
            if (tags is null)
                return default;

            var modi = LanguageModifier.None;
            var firstLang = LanguageName.Japanese;
            var language = default(List<LanguageName>);
            var lanNA = false;
            foreach (var item in tags[Namespace.Language])
            {
                if (item.State.GetPowerState() == TagState.LowPower)
                    continue;
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
                        lanNA = true;
                    }
                    else if (!lanNA)
                    {
                        if (!Enum.TryParse<LanguageName>(item.Content.Content, true, out var currentLang))
                            currentLang = LanguageName.Other;
                        if (language is null)
                        {
                            language = new List<LanguageName>(1);
                            firstLang = currentLang;
                        }
                        else
                            language.Add(currentLang);
                    }
                    continue;
                }
            }
            if (lanNA)
                return new Language(default, null, modi);
            else if (language is null)
                return new Language(LanguageName.Japanese, Array.Empty<LanguageName>(), modi);
            else if (language.Count == 0)
                return new Language(firstLang, Array.Empty<LanguageName>(), modi);
            else
                return new Language(firstLang, language.ToArray(), modi);
        }
    }

    public readonly struct Language : IEquatable<Language>
    {
        internal Language(LanguageName firstName, LanguageName[] otherNames, LanguageModifier modifier)
        {
            this.firstName = firstName;
            this.otherNames = otherNames;
            this.Modifier = modifier;
        }

        private readonly LanguageName firstName;
        private readonly LanguageName[] otherNames;

        public IEnumerable<LanguageName> Names
        {
            get
            {
                if (this.otherNames is null)
                    yield break;
                yield return this.firstName;
                foreach (var item in this.otherNames)
                    yield return item;
            }
        }

        public LanguageModifier Modifier { get; }

        public override string ToString()
        {
            if (this.otherNames is null) // rare
                return LocalizedStrings.Language.Names.NotApplicable;

            string name;
            if (this.otherNames.Length == 0) // most cases
                name = this.firstName.ToFriendlyNameString();
            else // very rare
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
            if (this.firstName != other.firstName)
                return false;
            if (this.otherNames is null)
                return other.otherNames is null;
            if (other.otherNames is null)
                return false;

            return this.otherNames.SequenceEqual(other.otherNames);
        }

        public override bool Equals(object obj) => obj is Language l && Equals(l);

        public override int GetHashCode()
        {
            var hash = this.Modifier.GetHashCode() + (int)this.firstName * 237;
            if (this.otherNames != null)
                foreach (var item in this.otherNames)
                    hash = unchecked(hash * 7 ^ item.GetHashCode());
            return hash;
        }
    }
}