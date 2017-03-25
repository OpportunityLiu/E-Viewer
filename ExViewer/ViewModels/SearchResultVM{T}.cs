using ExClient;
using ExViewer.Database;
using ExViewer.Settings;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Linq;
using EhTagTranslatorClient;

namespace ExViewer.ViewModels
{
    public class AutoCompletion
    {
        private AutoCompletion(string content)
        {
            this.Content = content;
        }

        public override string ToString()
        {
            return this.Content;
        }

        public string Content { get; private set; }

        internal static IEnumerable<AutoCompletion> GetCompletions(string input)
        {
            if(string.IsNullOrWhiteSpace(input))
                return getCompletionsWithEmptyInput();
            var quoteCount = input.Count(c => c == '\"');
            if(quoteCount % 2 == 0)
                return getCompletionsWithQuoteFinished(input);
            else
                return getCompletionsWithQuoteUnfinished(input);
        }

        static AutoCompletion()
        {
            var ns = Enum.GetNames(typeof(Namespace)).ToList();
            ns.Remove(Namespace.Misc.ToString());
            ns.Remove(Namespace.Unknown.ToString());
            for(var i = 0; i < ns.Count; i++)
            {
                ns[i] = ns[i].ToLowerInvariant();
            }
            ns.Add("uploader");
            namedNamespaces = ns.AsReadOnly();
        }

        private static readonly IReadOnlyList<string> namedNamespaces;

        private static IEnumerable<AutoCompletion> getCompletionsWithEmptyInput()
        {
            yield break;
        }

        private static IEnumerable<AutoCompletion> getCompletionsWithQuoteUnfinished(string input)
        {
            var lastChar = input[input.Length - 1];
            switch(lastChar)
            {
            case ' ':
            case ':':
            case '"':
                yield break;
            case '$':
                yield return new AutoCompletion($"{input}\"");
                yield break;
            case '-':
            default:
                yield return new AutoCompletion($"{input}\"");
                yield return new AutoCompletion($"{input}$\"");
                yield break;
            }
        }

        private static IEnumerable<AutoCompletion> getCompletionsWithQuoteFinished(string input)
        {
            var lastChar = input[input.Length - 1];
            switch(lastChar)
            {
            case ' ':
            case '-':
                // Too many results
                //foreach(var item in namedNamespaces)
                //{
                //    yield return new AutoCompletion($"{input}{item}:");
                //}
                yield break;
            case ':':
                yield return new AutoCompletion($"{input}\"");
                yield break;
            case '"':
            case '$':
                yield break;
            default:
                var index = input.LastIndexOf(' ') + 1;
                var lastTerm = input.Substring(index);
                if(lastTerm.Length > 0 && lastTerm.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    var beforeLastTerm = input.Substring(0, input.Length - lastTerm.Length);
                    foreach(var item in namedNamespaces)
                    {
                        if(item.StartsWith(lastTerm, StringComparison.OrdinalIgnoreCase))
                            yield return new AutoCompletion($"{beforeLastTerm}{item}:");
                    }
                }
                yield break;
            }
        }
    }

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

        public static TagRecord<Record> GetRecord(string highlight, Record tag)
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
            else if(tag.Translated.Text.Contains(highlight))
            {
                if(tag.Translated.Text.StartsWith(highlight))
                {
                    score += highlight.Length * 65536 * 16 / tag.Translated.Text.Length;
                }
                else
                {
                    score += highlight.Length * 65536 / tag.Translated.Text.Length;
                }
            }
            score *= nsFactor[tag.Namespace];
            if(score == 0)
                return null;
            else
                return new EhTagRecord(highlight, tag, score);
        }

        private class EhTagRecord : TagRecord<Record>
        {
            public EhTagRecord(string highlight, Record tag, int score) : base(highlight, tag, score)
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
                    return new EhWikiTagRecord(highlight, tag, score);
            }

            using(var db = EhTagClient.Client.CreateDatabase())
            {
                var r = db.Tags.Where(t => t.TagConetnt.Contains(highlight)).ToList();
                return r.Select(t => getRecord(t.AsTag()));
            }
        }

        private class EhWikiTagRecord : TagRecord<Tag>
        {
            public EhWikiTagRecord(string highlight, Tag tag, int score) : base(highlight, tag, score)
            {
            }

            public override string TagToString()
            {
                return Tag.ToSearchTerm();
            }

            public override string Title => Tag.Content;

            public override string Caption => EhWikiClient.Client.Get(Tag.Content)?.Japanese ?? "";

            public override string AdditionalInfo => Tag.Namespace.ToFriendlyNameString();
        }
    }

    public abstract class SearchResultVM<T> : ViewModelBase
        where T : SearchResultBase
    {
        private T searchResult;

        public T SearchResult
        {
            get => this.searchResult;
            protected set
            {
                if(this.searchResult != null)
                    this.searchResult.LoadMoreItemsException -= this.SearchResult_LoadMoreItemsException;
                Set(ref this.searchResult, value);
                if(this.searchResult != null)
                    this.searchResult.LoadMoreItemsException += this.SearchResult_LoadMoreItemsException;
            }
        }

        private Gallery selectedGallery;

        public Gallery SelectedGallery
        {
            get => this.selectedGallery;
            protected set => Set(ref this.selectedGallery, value);
        }

        private void SearchResult_LoadMoreItemsException(IncrementalLoadingCollection<Gallery> sender, LoadMoreItemsExceptionEventArgs args)
        {
            if(!RootControl.RootController.Available)
                return;
            RootControl.RootController.SendToast(args.Exception, typeof(SearchPage));
            args.Handled = true;
        }

        internal static void AddHistory(string content)
        {
            using(var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Add(SearchHistory.Create(content));
                db.SaveChanges();
            }
        }

        internal RelayCommand<SearchHistory> DeleteHistory
        {
            get;
        } = new RelayCommand<SearchHistory>(sh =>
        {
            using(var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Remove(sh);
                db.SaveChanges();
            }
        }, sh => sh != null);

        private class TagRecordEqulityComparer : IEqualityComparer<ITagRecord>
        {
            public bool Equals(ITagRecord x, ITagRecord y)
            {
                return x.TagToString() == y.TagToString();
            }

            public int GetHashCode(ITagRecord obj)
            {
                return (obj?.TagToString() ?? "").GetHashCode();
            }
        }

        private static readonly IEqualityComparer<ITagRecord> tagComparer = new TagRecordEqulityComparer();

        internal IAsyncOperation<IReadOnlyList<object>> LoadSuggestion(string input)
        {
            return Task.Run<IReadOnlyList<object>>(() =>
            {
                var historyKeyword = input?.Trim() ?? "";
                using(var db = new SearchHistoryDb())
                {
                    var history = ((IEnumerable<SearchHistory>)db.SearchHistorySet
                                                                 .Where(sh => sh.Content.Contains(historyKeyword))
                                                                 .OrderByDescending(sh => sh.Time))
                                        .Distinct()
                                        .Select(sh => sh.SetHighlight(historyKeyword));
                    var lastword = historyKeyword.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    var dictionary = Enumerable.Empty<ITagRecord>();
                    if(lastword != null)
                    {
                        var previous = historyKeyword.Substring(0, historyKeyword.Length - lastword.Length);
                        dictionary = EhTagDatabase.Dictionary
                            .SelectMany(dic
                                => dic.Value.Values
                                    .Select<Record, ITagRecord>(va => TagRecordFactory.GetRecord(lastword, va))
                                    .Where(t => t != null)
                            )
                            .Concat(TagRecordFactory.GetRecords(lastword)).Where(t => t != null)
                            .OrderByDescending(t => t.Score).Take(10).Distinct(tagComparer).Select(tag => tag.SetPrevious(previous));
                    }
                    try
                    {
                        return ((IEnumerable<object>)AutoCompletion.GetCompletions(input)).Concat(dictionary).Concat(history).ToList().AsReadOnly();
                    }
                    catch(InvalidOperationException)
                    {
                        //Collection changed
                        return null;
                    }
                }
            }).AsAsyncOperation();
        }

        internal bool AutoCompleteFinished(object selectedSuggestion)
        {
            if(selectedSuggestion is SearchHistory)
                return true;
            return false;
        }

        public IAsyncAction ClearHistoryAsync()
        {
            return Run(async token =>
            {
                using(var db = new SearchHistoryDb())
                {
                    db.SearchHistorySet.RemoveRange(db.SearchHistorySet);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}