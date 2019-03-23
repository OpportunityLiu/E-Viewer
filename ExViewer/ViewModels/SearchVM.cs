using ExClient;
using ExClient.Search;
using ExViewer.Database;
using ExViewer.Settings;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using System;
using Windows.Foundation;

namespace ExViewer.ViewModels
{
    public sealed class SearchVM : SearchResultVM<CategorySearchResult>
    {
        public static IAsyncAction InitAsync()
        {
            var defaultVM = GetVM(string.Empty);
            return defaultVM.SearchResult.LoadNextPage();
        }

        private static AutoFillCacheStorage<string, SearchVM> Cache = AutoFillCacheStorage.Create((string query) =>
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
                search = (CategorySearchResult)((ExClient.Launch.SearchLaunchResult)handle.GetResults()).Data;
            }
            var vm = new SearchVM(search);
            HistoryDb.Add(new HistoryRecord
            {
                Type = HistoryRecordType.Search,
                Uri = vm.SearchResult.SearchUri,
                Title = vm.Keyword,
            });
            return vm;
        }, 10);

        public static SearchVM GetVM(string query) => Cache.GetOrCreateAsync(query ?? string.Empty).GetResults();

        public static SearchVM GetVM(CategorySearchResult searchResult)
        {
            var vm = new SearchVM(searchResult ?? throw new ArgumentNullException(nameof(searchResult)));
            HistoryDb.Add(new HistoryRecord
            {
                Type = HistoryRecordType.Search,
                Uri = vm.SearchResult.SearchUri,
                Title = vm.Keyword,
            });
            Cache[vm.SearchQuery] = vm;
            return vm;
        }

        private SearchVM(CategorySearchResult searchResult)
            : base(searchResult)
        {
            Commands.Add(nameof(Search), Command<string>.Create(async (sender, queryText) =>
            {
                var that = (SearchVM)sender.Tag;
                if (SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = that.category;
                    SettingCollection.Current.DefaultSearchString = queryText;
                }
                var vm = GetVM(Client.Current.Search(queryText, that.category, that.advancedSearch));
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }));
        }

        public override void SetQueryWithSearchResult()
        {
            base.SetQueryWithSearchResult();
            Category = SearchResult.Category;
            AdvancedSearch = (SearchResult as AdvancedSearchResult)?.AdvancedSearch ?? new AdvancedSearchOptions();
            FileSearch = SearchResult as FileSearchResult;
        }

        private Category category;

        public Category Category
        {
            get => category;
            set => Set(ref category, value);
        }

        private AdvancedSearchOptions advancedSearch;
        public AdvancedSearchOptions AdvancedSearch
        {
            get => advancedSearch;
            private set => Set(ref advancedSearch, value);
        }

        private FileSearchResult fileSearch;
        public FileSearchResult FileSearch
        {
            get => fileSearch;
            private set => Set(ref fileSearch, value);
        }
    }
}
