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
            for(int i = 0; i < ns.Count; i++)
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

    public abstract class SearchResultVM<T> : ViewModelBase
        where T : SearchResultBase
    {
        private T searchResult;

        public T SearchResult
        {
            get
            {
                return this.searchResult;
            }
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
            get
            {
                return this.selectedGallery;
            }
            protected set
            {
                Set(ref this.selectedGallery, value);
            }
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
                    return ((IEnumerable<object>)AutoCompletion.GetCompletions(input)).Concat(history).ToList().AsReadOnly();
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
