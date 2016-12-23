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
                var query = JsonConvert.SerializeObject(new SearchResultData()
                {
                    KeyWord = searchVM.KeyWord,
                    Category = searchVM.Category,
                    AdvancedSearch = searchVM.AdvancedSearch
                });
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
            keyWord = searchResult.KeyWord;
            category = searchResult.Category;
            advancedSearch = searchResult.AdvancedSearch;
            SearchResult = searchResult;
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
                GalleryVM.AddGallery(g);
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
