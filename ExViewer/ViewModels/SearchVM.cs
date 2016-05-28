using ExClient;
using ExViewer.Settings;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private static class Cache
        {

            private static CacheStorage<string, SearchResult> srCache = new CacheStorage<string, SearchResult>(query =>
                {
                    var data = JsonConvert.DeserializeObject<SearchResultData>(query);
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
                srCache.Add(query, searchResult);
                return query;
            }
        }


        public SearchVM(string parameter)
            : this()
        {
            if(parameter == null)
            {
                keyWord = SettingCollection.Current.DefaultSearchString;
                category = SettingCollection.Current.DefaultSearchCategory;
                searchResult = Cache.GetSearchResult(Cache.GetSearchQuery(keyWord, category));
            }
            else
            {
                var q = JsonConvert.DeserializeObject<SearchResultData>(parameter);
                keyWord = q.KeyWord;
                category = q.Category;
                advancedSearch = q.AdvancedSearch;
                searchResult = Cache.GetSearchResult(parameter);
            }
        }

        private SearchVM(string keyWord, Category category, AdvancedSearchOptions advancedSearch)
            : this()
        {
            this.searchResult = Cache.GetSearchResult(Cache.GetSearchQuery(keyWord, category, advancedSearch));
            this.keyWord = keyWord;
            this.category = category;
            this.advancedSearch = advancedSearch;
        }

        private SearchVM()
        {
            SettingCollection.SetHah();
            Search = new RelayCommand(() =>
            {
                if(SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = category;
                    SettingCollection.Current.DefaultSearchString = keyWord;
                }
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), Cache.GetSearchQuery(keyWord, category, advancedSearch));
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                GalleryVM.AddGallery(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            });
        }

        public RelayCommand Search
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
                Set(ref searchResult, value);
            }
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

    }
}
