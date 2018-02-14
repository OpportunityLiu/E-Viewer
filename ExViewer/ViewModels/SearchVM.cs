using ExClient;
using ExViewer.Database;
using ExViewer.Settings;
using ExViewer.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Linq;
using Opportunity.MvvmUniverse.Commands;
using ExClient.Search;
using ExClient.Galleries;
using Opportunity.MvvmUniverse.Collections;

namespace ExViewer.ViewModels
{
    public class SearchVM : SearchResultVM<CategorySearchResult>
    {
        private static class Cache
        {
            private static CacheStorage<string, SearchVM> srCache = new CacheStorage<string, SearchVM>(query =>
            {
                var search = default(CategorySearchResult);
                if (string.IsNullOrEmpty(query))
                {
                    var keyword = SettingCollection.Current.DefaultSearchString;
                    var category = SettingCollection.Current.DefaultSearchCategory;
                    search = Client.Current.Search(keyword, category);
                }
                else
                {
                    var uri = new Uri(query);

                    var handle = ExClient.Launch.UriLauncher.HandleAsync(uri);
                    if (handle.Status != AsyncStatus.Completed)
                        throw new ArgumentException();
                    search = (CategorySearchResult)((ExClient.Launch.SearchLaunchResult)handle.GetResults()).Data;
                }
                var vm = new SearchVM(search);
                AddHistory(vm.Keyword);
                return vm;
            }, 10);

            public static SearchVM GetSearchVM(string query)
            {
                return srCache.Get(query);
            }

            public static string AddSearchVM(SearchVM searchVM)
            {
                var query = searchVM.SearchQuery.ToString();
                AddHistory(searchVM.Keyword);
                srCache.Add(query, searchVM);
                return query;
            }
        }

        public static IAsyncAction InitAsync()
        {
            var defaultVM = GetVM(string.Empty);
            return defaultVM.SearchResult.LoadMoreItemsAsync(40).AsTask().AsAsyncAction();
        }

        public static SearchVM GetVM(string parameter)
        {
            return Cache.GetSearchVM(parameter ?? string.Empty);
        }

        public static SearchVM GetVM(CategorySearchResult searchResult)
        {
            var vm = new SearchVM(searchResult ?? throw new ArgumentNullException(nameof(searchResult)));
            Cache.AddSearchVM(vm);
            return vm;
        }

        private SearchVM(CategorySearchResult searchResult)
            : this()
        {
            this.SearchResult = searchResult;
            SetQueryWithSearchResult();
        }

        private SearchVM()
        {
            this.Search.Tag = this;
        }

        public Command<string> Search { get; } = Command.Create<string>(async (sender, queryText) =>
        {
            var that = (SearchVM)sender.Tag;
            if (SettingCollection.Current.SaveLastSearch)
            {
                SettingCollection.Current.DefaultSearchCategory = that.category;
                SettingCollection.Current.DefaultSearchString = queryText;
            }
            var vm = GetVM(Client.Current.Search(queryText, that.category, that.advancedSearch));
            await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery.ToString());
        });

        public void SetQueryWithSearchResult()
        {
            this.Keyword = this.SearchResult.Keyword;
            this.Category = this.SearchResult.Category;
            this.AdvancedSearch = (this.SearchResult as AdvancedSearchResult)?.AdvancedSearch ?? new AdvancedSearchOptions();
            this.FileSearch = this.SearchResult as FileSearchResult;
        }

        private string keyword;

        public string Keyword
        {
            get => this.keyword;
            set => Set(ref this.keyword, value);
        }

        private Category category;

        public Category Category
        {
            get => this.category;
            set => Set(ref this.category, value);
        }

        private AdvancedSearchOptions advancedSearch;
        public AdvancedSearchOptions AdvancedSearch
        {
            get => this.advancedSearch;
            private set => Set(ref this.advancedSearch, value);
        }

        private FileSearchResult fileSearch;
        public FileSearchResult FileSearch
        {
            get => this.fileSearch;
            private set => Set(ref this.fileSearch, value);
        }

        public Uri SearchQuery => this.SearchResult.SearchUri;
    }
}
