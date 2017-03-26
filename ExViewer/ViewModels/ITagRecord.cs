using ExClient;
using System.Collections.Generic;
using System.Linq;
using WikiRecod = EhWikiClient.Record;
using WikiClient = EhWikiClient.Client;
using TransClient = EhTagTranslatorClient.Client;
using TransRecord = EhTagTranslatorClient.Record;

namespace ExViewer.ViewModels
{
    public interface ITagRecord
    {
        string AdditionalInfo { get; }
        string Caption { get; }
        string Highlight { get; }
        string Previous { get; set; }
        int Score { get; }
        string Title { get; }

        string TagToString();
        ITagRecord SetPrevious(string p);
        string ToString();
    }

    public abstract class TagRecord<T> : ITagRecord
    {
        public TagRecord(string highlight, T tag, int score)
        {
            this.Highlight = highlight;
            this.Tag = tag;
            this.Score = score;
        }

        public T Tag { get; }

        public string Highlight { get; }

        public int Score { get; }

        public string Previous { get; set; }

        public abstract string Title { get; }

        public abstract string Caption { get; }

        public abstract string AdditionalInfo { get; }

        public virtual string TagToString()
        {
            return Tag.ToString();
        }

        public override string ToString()
        {
            return Previous + TagToString();
        }

        ITagRecord ITagRecord.SetPrevious(string p)
        {
            this.Previous = p;
            return this;
        }
    }

    public static class TagRecordFactory
    {
        private static Dictionary<Namespace, int> nsFactor = new Dictionary<Namespace, int>()
        {
            [Namespace.Unknown] = 1,
            [Namespace.Reclass] = 4,
            [Namespace.Language] = 16,
            [Namespace.Parody] = 24,
            [Namespace.Character] = 12,
            [Namespace.Group] = 2,
            [Namespace.Artist] = 2,
            [Namespace.Male] = 16,
            [Namespace.Female] = 16,
            [Namespace.Misc] = 20
        };

        public static IEnumerable<TagRecord<TransRecord>> GetTranslatedRecords(string highlight)
        {
            TagRecord<TransRecord> getRecord(TransRecord tag)
            {
                var score = 0;
                if(tag.Original.Contains(highlight))
                {
                    if(tag.Original.StartsWith(highlight))
                    {
                        score += highlight.Length * 65536 * 16 / tag.Original.Length;
                    }
                    else
                    {
                        score += highlight.Length * 65536 / tag.Original.Length;
                    }
                }
                else if(tag.TranslatedStr.Contains(highlight))
                {
                    if(tag.TranslatedStr.StartsWith(highlight))
                    {
                        score += highlight.Length * 65536 * 16 / tag.Translated.Text.Length;
                    }
                    else
                    {
                        score += highlight.Length * 65536 / tag.Translated.Text.Length;
                    }
                }
                score *= nsFactor[tag.Namespace];
                return new TransTagRecord(highlight, tag, score);
            }

            using(var db = TransClient.CreateDatabase())
            {
                var r = db.Tags.Where(t => t.Original.Contains(highlight) || t.TranslatedStr.Contains(highlight)).ToList();
                return r.Select(t => getRecord(t));
            }
        }

        private class TransTagRecord : TagRecord<TransRecord>
        {
            public TransTagRecord(string highlight, TransRecord tag, int score) : base(highlight, tag, score)
            {
            }

            public override string TagToString()
            {
                if(Tag.Namespace != Namespace.Misc)
                    return $"{Tag.Namespace.ToString().ToLowerInvariant()}:\"{Tag.Original}$\"";
                else
                    return $"\"{Tag.Original}$\"";
            }

            public override string Title => Tag.Original;

            public override string Caption => Tag.Translated.Text;

            public override string AdditionalInfo => Tag.Namespace.ToFriendlyNameString();
        }

        public static IEnumerable<TagRecord<Tag>> GetRecords(string highlight)
        {
            TagRecord<Tag> getRecord(Tag tag)
            {
                var score = 0;
                if(tag.Content.Contains(highlight))
                {
                    if(tag.Content.StartsWith(highlight))
                    {
                        score += highlight.Length * 65536 * 16 / tag.Content.Length;
                    }
                    else
                    {
                        score += highlight.Length * 65536 / tag.Content.Length;
                    }
                }
                score *= nsFactor[tag.Namespace];
                if(score == 0)
                    return null;
                else
                    return new EhTagRecord(highlight, tag, score);
            }

            using(var db = EhTagClient.Client.CreateDatabase())
            {
                var r = db.Tags.Where(t => t.TagConetnt.Contains(highlight)).ToList();
                return r.Select(t => getRecord(t.AsTag()));
            }
        }

        private class EhTagRecord : TagRecord<Tag>
        {
            public EhTagRecord(string highlight, Tag tag, int score) : base(highlight, tag, score)
            {
            }

            public override string TagToString()
            {
                return Tag.ToSearchTerm();
            }

            public override string Title => Tag.Content;

            public override string Caption => WikiClient.Get(Tag.Content)?.Japanese ?? "";

            public override string AdditionalInfo => Tag.Namespace.ToFriendlyNameString();
        }
    }
}