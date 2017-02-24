using System;
using System.Linq;

namespace ExClient
{
    public enum LanguageModifier
    {
        None,
        Translated,
        Rewrite
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
            var language = (string)null;
            var lanFinished = false;
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
                        language = "N/A";
                        lanFinished = true;
                    }
                    else if(!lanFinished)
                    {
                        if(language == null)
                            language = item.Content;
                        else
                            language += ", " + item.Content;
                    }
                    continue;
                }
            }
            return new Language(language ?? "japanese", modi);
        }

        public Language(string name, LanguageModifier modifier)
        {
            var ca = name.ToCharArray();
            ca[0] = char.ToUpperInvariant(ca[0]);
            this.name = new string(ca);
            Modifier = modifier;
        }

        private string name;

        public string Name => name ?? "Japanese";

        public LanguageModifier Modifier
        {
            get;
        }

        public override string ToString()
        {
            switch(Modifier)
            {
            case LanguageModifier.Translated:
                return $"{Name} TR";
            case LanguageModifier.Rewrite:
                return $"{Name} RW";
            default:
                return Name;
            }
        }

        public bool Equals(Language other)
        {
            return this.Name == other.Name && this.Modifier == other.Modifier;
        }

        public override bool Equals(object obj)
        {
            if(obj == null || !(obj is Language))
            {
                return false;
            }
            return Equals((Language)obj);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ (int)this.Modifier;
        }
    }
}