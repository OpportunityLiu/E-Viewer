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
            Span<LanguageName> language = stackalloc LanguageName[32];
            var langCount = 0;
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
                        langCount = 0;
                        lanNA = true;
                    }
                    else if (!lanNA)
                    {
                        if (Enum.TryParse<LanguageName>(item.Content.Content, true, out var lan))
                        {
                            language[langCount] = lan;
                            langCount++;
                        }
                        else
                        {
                            language[langCount] = LanguageName.Other;
                            langCount++;
                        }
                    }
                    continue;
                }
            }
            if (lanNA)
                return new Language(default, null, modi);
            else if (langCount == 0)
                return new Language(LanguageName.Japanese, Array.Empty<LanguageName>(), modi);
            else if (langCount == 1)
                return new Language(language[0], Array.Empty<LanguageName>(), modi);
            else
                return new Language(language[0], language.Slice(0, langCount).ToArray(), modi);
        }
    }

    public readonly struct Language : IEquatable<Language>
    {
        internal Language(LanguageName firstName, LanguageName[] names, LanguageModifier modifier)
        {
            this.firstName = firstName;
            this.names = names;
            this.Modifier = modifier;
        }

        private readonly LanguageName firstName;
        private readonly LanguageName[] names;

        public IReadOnlyList<LanguageName> Names
        {
            get
            {
                if (this.names is null)
                    return Array.Empty<LanguageName>();
                if (this.names.Length == 0)
                    return new[] { this.firstName };
                return this.names;
            }
        }

        public LanguageModifier Modifier { get; }

        public override string ToString()
        {
            if (this.names is null) // rare
                return LocalizedStrings.Language.Names.NotApplicable;

            string name;
            if (this.names.Length == 0) // most cases
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
            if (this.names is null)
                return other.names is null;
            if (other.names is null)
                return false;

            return this.names.SequenceEqual(other.names);
        }

        public override bool Equals(object obj) => obj is Language l && Equals(l);

        public override int GetHashCode()
        {
            var hash = this.Modifier.GetHashCode() + (int)this.firstName * 237;
            if (this.names != null)
                foreach (var item in this.names)
                    hash = unchecked(hash * 7 ^ item.GetHashCode());
            return hash;
        }
    }
}