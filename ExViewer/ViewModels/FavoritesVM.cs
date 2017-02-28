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
    public class FavoritesVM : SearchResultVM<FavoritesSearchResult>
    {
        private class SearchResultData
        {
            public string Keyword
            {
                get; set;
            }

            public int CategoryIndex
            {
                get; set;
            }
        }

        private static class Cache
        {
            private static CacheStorage<string, FavoritesVM> srCache = new CacheStorage<string, FavoritesVM>(query =>
            {
                var vm = new FavoritesVM(query);
                AddHistory(vm.Keyword);
                return vm;
            }, 10);

            public static FavoritesVM GetSearchVM(string query)
            {
                return srCache.Get(query);
            }

            public static string AddSearchVM(FavoritesVM searchVM)
            {
                var query = searchVM.SearchQuery;
                AddHistory(searchVM.Keyword);
                srCache.Add(query, searchVM);
                return query;
            }
        }

        public static string GetSearchQuery(string keyword)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                Keyword = keyword
            });
        }

        public static string GetSearchQuery(string keyword, FavoriteCategory filter)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                Keyword = keyword,
                CategoryIndex = filter?.Index ?? -1
            });
        }

        public static FavoritesVM GetVM(string parameter)
        {
            return Cache.GetSearchVM(parameter ?? string.Empty);
        }

        public static FavoritesVM GetVM(FavoritesSearchResult searchResult)
        {
            var vm = new FavoritesVM(searchResult);
            Cache.AddSearchVM(vm);
            return vm;
        }

        private FavoritesVM(FavoritesSearchResult searchResult)
            : this()
        {
            this.SearchResult = searchResult;
            SetQueryWithSearchResult();
        }

        private FavoritesVM(string parameter)
            : this()
        {
            if(string.IsNullOrEmpty(parameter))
            {
                this.keyword = "";
                this.category = FavoriteCategory.All;
            }
            else
            {
                var q = JsonConvert.DeserializeObject<SearchResultData>(parameter);
                this.keyword = q.Keyword;
                if(q.CategoryIndex > 0)
                    this.category = Client.Current.Favorites[q.CategoryIndex];
                else
                    this.category = FavoriteCategory.All;
            }
            this.SearchResult = Client.Current.Favorites.Search(this.keyword, this.category);
        }

        private FavoritesVM()
        {
            this.Search = new RelayCommand<string>(queryText =>
            {
                RootControl.RootController.Frame.Navigate(typeof(FavoritesPage), GetSearchQuery(queryText, this.category));
            });
            this.Open = new RelayCommand<Gallery>(g =>
            {
                this.SelectedGallery = g;
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

        public void SetQueryWithSearchResult()
        {
            this.Keyword = this.SearchResult.Keyword;
            this.Category = this.SearchResult.Category;
        }

        private string keyword;

        public string Keyword
        {
            get
            {
                return this.keyword;
            }
            set
            {
                Set(ref this.keyword, value);
            }
        }

        private FavoriteCategory category;

        public FavoriteCategory Category
        {
            get
            {
                return this.category;
            }
            set
            {
                Set(ref this.category, value);
            }
        }

        public string SearchQuery => GetSearchQuery(this.keyword, this.category);
    }
}
