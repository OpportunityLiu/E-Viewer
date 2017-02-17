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
            var ns = Enum.GetNames(typeof(NameSpace)).ToList();
            ns.Remove(NameSpace.Misc.ToString());
            for(int i = 0; i < ns.Count; i++)
            {
                ns[i] = ns[i].ToLowerInvariant();
            }
            ns.Add("uploader");
            namedNameSpaces = ns.AsReadOnly();
        }

        private static readonly IReadOnlyList<string> namedNameSpaces;

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
                //foreach(var item in namedNameSpaces)
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
                    foreach(var item in namedNameSpaces)
                    {
                        if(item.StartsWith(lastTerm, StringComparison.OrdinalIgnoreCase))
                            yield return new AutoCompletion($"{beforeLastTerm}{item}:");
                    }
                }
                yield break;
            }
        }
    }

    public class SearchVM : ViewModelBase
    {
        private class SearchResultData
        {
            public string KeyWord
            {
                get; set;
            }

            public Category Category
            {
                get; set;
            }

            public AdvancedSearchOptions AdvancedSearch
            {
                get; set;
            }
        }

        internal static class Cache
        {
            private static CacheStorage<string, SearchVM> srCache = new CacheStorage<string, SearchVM>(query =>
                {
                    var vm = new SearchVM(query);
                    AddHistory(vm.KeyWord);
                    return vm;
                }, 10);

            public static SearchVM GetSearchVM(string query)
            {
                return srCache.Get(query);
            }

            public static string AddSearchVM(SearchVM searchVM)
            {
                var query = searchVM.SearchQuery; 
                AddHistory(searchVM.KeyWord);
                srCache.Add(query, searchVM);
                return query;
            }
        }

        public static string GetSearchQuery(string keyWord)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord
            });
        }

        public static string GetSearchQuery(string keyWord, Category filter)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord,
                Category = filter
            });
        }

        public static string GetSearchQuery(string keyWord, Category filter, AdvancedSearchOptions advancedSearch)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                KeyWord = keyWord,
                Category = filter,
                AdvancedSearch = advancedSearch
            });
        }

        public static IAsyncAction InitAsync()
        {
            var defaultVM = GetVM(string.Empty);
            return defaultVM.searchResult.LoadMoreItemsAsync(40).AsTask().AsAsyncAction();
        }

        public static SearchVM GetVM(string parameter)
        {
            return Cache.GetSearchVM(parameter ?? string.Empty);
        }

        public static SearchVM GetVM(SearchResult searchResult)
        {
            var vm = new SearchVM(searchResult);
            Cache.AddSearchVM(vm);
            return vm;
        }

        private SearchVM(SearchResult searchResult)
            : this()
        {
            SearchResult = searchResult;
            SetQueryWithSearchResult();
        }

        private SearchVM(string parameter)
            : this()
        {
            if(string.IsNullOrEmpty(parameter))
            {
                keyWord = SettingCollection.Current.DefaultSearchString;
                category = SettingCollection.Current.DefaultSearchCategory;
                advancedSearch = new AdvancedSearchOptions();
            }
            else
            {
                var q = JsonConvert.DeserializeObject<SearchResultData>(parameter);
                keyWord = q.KeyWord;
                category = q.Category;
                advancedSearch = q.AdvancedSearch;
            }
            SearchResult = Client.Current.Search(keyWord, category, advancedSearch);
        }

        private SearchVM()
        {
            Search = new RelayCommand<string>(queryText =>
            {
                if(SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = category;
                    SettingCollection.Current.DefaultSearchString = queryText;
                }
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), GetSearchQuery(queryText, category, advancedSearch));
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                SelectedGallery = g;
                GalleryVM.GetVM(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            }, g => g != null);
        }

        public RelayCommand<string> Search
        {
            get;
        }

        public RelayCommand<Gallery> Open
        {
            get;
        }

        private Gallery selectedGallery;

        public Gallery SelectedGallery
        {
            get
            {
                return selectedGallery;
            }
            private set
            {
                Set(ref selectedGallery, value);
            }
        }

        private SearchResult searchResult;

        public SearchResult SearchResult
        {
            get
            {
                return searchResult;
            }
            private set
            {
                if(searchResult != null)
                    searchResult.LoadMoreItemsException -= SearchResult_LoadMoreItemsException;
                Set(ref searchResult, value);
                if(searchResult != null)
                    searchResult.LoadMoreItemsException += SearchResult_LoadMoreItemsException;
            }
        }

        private void SearchResult_LoadMoreItemsException(IncrementalLoadingCollection<Gallery> sender, LoadMoreItemsExceptionEventArgs args)
        {
            if(!RootControl.RootController.Available)
                return;
            RootControl.RootController.SendToast(args.Exception, typeof(SearchPage));
            args.Handled = true;
        }

        public void SetQueryWithSearchResult()
        {
            KeyWord = searchResult.KeyWord;
            Category = searchResult.Category;
            var adv = searchResult.AdvancedSearch;
            if(adv == null)
                AdvancedSearch = new AdvancedSearchOptions();
            else
                AdvancedSearch = adv.Clone(false);
        }

        private string keyWord;

        public string KeyWord
        {
            get
            {
                return keyWord;
            }
            set
            {
                Set(ref keyWord, value);
            }
        }

        private Category category;

        public Category Category
        {
            get
            {
                return category;
            }
            set
            {
                Set(ref category, value);
            }
        }

        private AdvancedSearchOptions advancedSearch;

        public AdvancedSearchOptions AdvancedSearch
        {
            get
            {
                return advancedSearch;
            }
            set
            {
                Set(ref advancedSearch, value);
            }
        }

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

        public async void ClearHistory()
        {
            using(var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.RemoveRange(db.SearchHistorySet);
                await db.SaveChangesAsync();
            }
        }

        public static void AddHistory(string content)
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

        public string SearchQuery => GetSearchQuery(this.keyWord, this.category, this.advancedSearch);
    }
}
