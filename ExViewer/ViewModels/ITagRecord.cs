using ExClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TransClient = EhTagTranslatorClient.Client;
using TransRecord = EhTagTranslatorClient.Record;
using WikiClient = EhWikiClient.Client;

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
            return Previous + TagToString() + " ";
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
            [Namespace.Language] = 25,
            [Namespace.Parody] = 15,
            [Namespace.Character] = 8,
            [Namespace.Group] = 4,
            [Namespace.Artist] = 4,
            [Namespace.Male] = 20,
            [Namespace.Female] = 20,
            [Namespace.Misc] = 20
        };

        private static TagRecord<Tag> getRecord(TransRecord tag, string highlight)
        {
            var score = 0;
            var io = tag.Original.IndexOf(highlight, StringComparison.OrdinalIgnoreCase);
            if (io != -1)
            {
                if (io == 0)
                    score = highlight.Length * 65536 * 16 / tag.Original.Length;
                else
                    score = highlight.Length * 65536 / tag.Original.Length;
            }
            var to = tag.TranslatedStr.IndexOf(highlight, StringComparison.OrdinalIgnoreCase);
            if (to != -1)
            {
                if (to == 0)
                    score = Math.Max(score, highlight.Length * 65536 * 16 / tag.TranslatedStr.Length);
                else
                    score = Math.Max(score, highlight.Length * 65536 / tag.TranslatedStr.Length);
            }
            score *= nsFactor[tag.Namespace];
            return new TagRecord(highlight, tag.AsTag(), score);
        }

        private static TagRecord<Tag> getRecord(EhTagClient.TagRecord tag, string highlight)
        {
            var score = 0;
            var c = tag.TagConetnt;
            var io = c.IndexOf(highlight, StringComparison.OrdinalIgnoreCase);
            if (io != -1)
            {
                if (io == 0)
                    score = highlight.Length * 65536 * 16 / c.Length;
                else
                    score = highlight.Length * 65536 / c.Length;
            }
            score *= nsFactor[tag.TagNamespace];
            return new TagRecord(highlight, tag.AsTag(), score);
        }

        public static IEnumerable<TagRecord<Tag>> GetTranslatedRecords(string highlight, Namespace ns)
        {
            using (var db = TransClient.CreateDatabase())
            {
                var r = default(List<TransRecord>);
                if (ns == Namespace.Unknown || ns == Namespace.Misc)
                    r = db.Tags.FromSql(@"SELECT * FROM 'Table' 
                                          WHERE Original LIKE {0} COLLATE nocase 
                                            Or TranslatedStr LIKE {0} COLLATE nocase", $"%{highlight}%")
                        .ToList();
                else
                    r = db.Tags.FromSql(@"SELECT * FROM 'Table' 
                                          WHERE Original LIKE {0} COLLATE nocase 
                                            Or TranslatedStr LIKE {0} COLLATE nocase", $"%{highlight}%")
                        .Where(t => t.Namespace == ns)
                        .ToList();
                return r.Select(t => getRecord(t, highlight));
            }
        }

        public static IEnumerable<TagRecord<Tag>> GetRecords(string highlight, Namespace ns)
        {
            using (var db = EhTagClient.Client.CreateDatabase())
            {
                var r = default(List<EhTagClient.TagRecord>);
                if (ns == Namespace.Unknown || ns == Namespace.Misc)
                    r = db.Tags.FromSql(@"SELECT * FROM 'TagTable' 
                                          WHERE TagConetnt LIKE {0} COLLATE nocase", $"%{highlight}%")
                               .ToList();
                else
                    r = db.Tags.FromSql(@"SELECT * FROM 'TagTable' 
                                          WHERE TagConetnt LIKE {0} COLLATE nocase", $"%{highlight}%")
                               .Where(t => t.TagNamespace == ns)
                               .ToList();
                return r.Select(t => getRecord(t, highlight));
            }
        }

        private class TagRecord : TagRecord<Tag>
        {
            public TagRecord(string highlight, Tag tag, int score) : base(highlight, tag, score)
            {
            }

            public override string TagToString()
            {
                return Tag.ToSearchTerm();
            }

            public override string Title => Tag.Content;

            public override string Caption => Tag.GetDisplayContent();

            public override string AdditionalInfo => Tag.Namespace.ToFriendlyNameString();
        }
    }
}