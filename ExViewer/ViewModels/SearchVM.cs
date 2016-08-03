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
            private static CacheStorage<string, SearchResult> srCache = new CacheStorage<string, SearchResult>(query =>
                {
                    var data = JsonConvert.DeserializeObject<SearchResultData>(query);
                    AddHistory(data.KeyWord);
                    return Client.Current.Search(data.KeyWord, data.Category, data.AdvancedSearch);
                }, 10
            );

            public static SearchResult GetSearchResult(string query)
            {
                return srCache.Get(query);
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

            public static string AddSearchResult(SearchResult searchResult)
            {
                var query = JsonConvert.SerializeObject(new SearchResultData()
                {
                    KeyWord = searchResult.KeyWord,
                    Category = searchResult.Category,
                    AdvancedSearch = searchResult.AdvancedSearch
                });
                AddHistory(searchResult.KeyWord);
                srCache.Add(query, searchResult);
                return query;
            }
        }

        public static IAsyncAction InitAsync()
        {
            var defaultVM = new SearchVM(null);
            return defaultVM.searchResult.LoadMoreItemsAsync(40).AsTask().AsAsyncAction();
        }

        public SearchVM(string parameter)
            : this()
        {
            if(parameter == null)
            {
                keyWord = SettingCollection.Current.DefaultSearchString;
                category = SettingCollection.Current.DefaultSearchCategory;
                advancedSearch = new AdvancedSearchOptions();
                SearchResult = Cache.GetSearchResult(Cache.GetSearchQuery(keyWord, category, advancedSearch));
            }
            else
            {
                var q = JsonConvert.DeserializeObject<SearchResultData>(parameter);
                keyWord = q.KeyWord;
                category = q.Category;
                advancedSearch = q.AdvancedSearch;
                SearchResult = Cache.GetSearchResult(parameter);
            }
        }

        private SearchVM()
        {
            SettingCollection.SetHah();
            Search = new RelayCommand<string>(queryText =>
            {
                if(SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = category;
                    SettingCollection.Current.DefaultSearchString = queryText;
                }
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), Cache.GetSearchQuery(queryText, category, advancedSearch));
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                GalleryVM.AddGallery(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            });
        }

        public RelayCommand<string> Search
        {
            get;
        }

        public RelayCommand<Gallery> Open
        {
            get;
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
            RootControl.RootController.SendToast(args.Exception, typeof(SearchPage));
            args.Handled = true;
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

        internal IAsyncOperation<IReadOnlyList<SearchHistory>> LoadSuggestion(string input)
        {
            return Task.Run(() =>
            {
                input = input?.Trim() ?? "";
                using(var db = new SearchHistoryDb())
                {
                    return (IReadOnlyList<SearchHistory>)
                    ((IEnumerable<SearchHistory>)
                        (db.SearchHistorySet
                            .Where(sh => sh.Content.Contains(input))
                            .OrderByDescending(sh => sh.Time)))
                        .Distinct()
                        .Select(sh => sh.SetHighlight(input))
                        .ToList();
                }
            }).AsAsyncOperation();
        }

        internal bool AutoComplateFinished(object selectedSuggestion)
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
    }
}
