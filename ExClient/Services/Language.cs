using ExClient.Galleries;
using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Services
{
    public static class LanguageExtension
    {
        private static readonly string[] _TechnicalTags = new[]
        {
            "rewrite",
            "translated"
        };

        private static readonly string[] _NaTags = new[]
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
            var firstLang = default(LanguageName?);
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
                    if (_NaTags.Contains(item.Content.Content))
                    {
                        lanNA = true;
                    }
                    else if (!lanNA)
                    {
                        if (!Enum.TryParse<LanguageName>(item.Content.Content, true, out var currentLang))
                            currentLang = LanguageName.Other;
                        if (firstLang is null)
                            firstLang = currentLang;
                        else
                        {
                            if (language is null)
                                language = new List<LanguageName>(1);
                            language.Add(currentLang);
                        }
                    }
                    continue;
                }
            }
            if (lanNA)
                return new Language(default, null, modi);
            else if (firstLang is null)
                return new Language(LanguageName.Japanese, Array.Empty<LanguageName>(), modi);
            else if (language is null)
                return new Language(firstLang.Value, Array.Empty<LanguageName>(), modi);
            else
                return new Language(firstLang.Value, language.ToArray(), modi);
        }
    }

    public readonly struct Language : IEquatable<Language>
    {
        internal Language(LanguageName firstName, LanguageName[] otherNames, LanguageModifier modifier)
        {
            _FirstName = firstName;
            _OtherNames = otherNames;
            Modifier = modifier;
        }

        public LanguageModifier Modifier { get; }
        private readonly LanguageName _FirstName;
        private readonly LanguageName[] _OtherNames;

        public IEnumerable<LanguageName> Names
        {
            get
            {
                if (_OtherNames is null)
                    yield break;
                yield return _FirstName;
                foreach (var item in _OtherNames)
                    yield return item;
            }
        }

        public override string ToString()
        {
            if (_OtherNames is null) // rare
                return LocalizedStrings.Language.Names.NotApplicable;

            string name;
            if (_OtherNames.Length == 0) // most cases
                name = _FirstName.ToFriendlyNameString();
            else // very rare
                name = string.Join(", ", Names.Select(LanguageNameExtension.ToFriendlyNameString));

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
            if (Modifier != other.Modifier)
                return false;
            if (_FirstName != other._FirstName)
                return false;
            if (_OtherNames is null)
                return other._OtherNames is null;
            if (other._OtherNames is null)
                return false;

            return _OtherNames.SequenceEqual(other._OtherNames);
        }

        public override bool Equals(object obj) => obj is Language l && Equals(l);

        public static bool operator ==(Language l, Language r) => l.Equals(r);
        public static bool operator !=(Language l, Language r) => !l.Equals(r);

        public override int GetHashCode()
        {
            var hash = Modifier.GetHashCode() + (int)_FirstName * 237;
            if (_OtherNames != null)
                foreach (var item in _OtherNames)
                    hash = unchecked(hash * 7 ^ item.GetHashCode());
            return hash;
        }
    }
}