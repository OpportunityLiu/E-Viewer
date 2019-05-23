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
    public sealed class WatchedVM : SearchResultVM<WatchedSearchResult>
    {
        public static IAsyncAction InitAsync()
        {
            var defaultVM = GetVM(string.Empty);
            return defaultVM.SearchResult.LoadNextPage();
        }

        private static AutoFillCacheStorage<string, WatchedVM> Cache = AutoFillCacheStorage.Create((string query) =>
        {
            var search = default(WatchedSearchResult);
            if (string.IsNullOrEmpty(query))
            {
                var keyword = SettingCollection.Current.DefaultSearchString;
                var category = SettingCollection.Current.DefaultSearchCategory;
                search = Client.Current.SearchWatched(keyword, category);
            }
            else
            {
                var uri = new Uri(query);

                var handle = ExClient.Launch.UriLauncher.HandleAsync(uri);
                search = (WatchedSearchResult)((ExClient.Launch.SearchLaunchResult)handle.Result).Data;
            }
            var vm = new WatchedVM(search);
            HistoryDb.Add(new HistoryRecord
            {
                Type = HistoryRecordType.Watched,
                Uri = vm.SearchResult.SearchUri,
                Title = vm.Keyword,
            });
            return vm;
        }, 10);

        public static WatchedVM GetVM(string query) => Cache.GetOrCreateAsync(query ?? string.Empty).GetResults();

        public static WatchedVM GetVM(WatchedSearchResult searchResult)
        {
            var vm = new WatchedVM(searchResult ?? throw new ArgumentNullException(nameof(searchResult)));
            HistoryDb.Add(new HistoryRecord
            {
                Type = HistoryRecordType.Watched,
                Uri = vm.SearchResult.SearchUri,
                Title = vm.Keyword,
            });
            Cache[vm.SearchQuery] = vm;
            return vm;
        }

        private WatchedVM(WatchedSearchResult searchResult)
            : base(searchResult)
        {
            Commands.Add(nameof(Search), Command<string>.Create(async (sender, queryText) =>
            {
                var that = (WatchedVM)sender.Tag;
                if (SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = that._Category;
                    SettingCollection.Current.DefaultSearchString = queryText;
                }
                var vm = GetVM(Client.Current.SearchWatched(queryText, that._Category, that._AdvancedSearch));
                await RootControl.RootController.Navigator.NavigateAsync(typeof(WatchedPage), vm.SearchQuery);
            }));
        }

        public override void SetQueryWithSearchResult()
        {
            base.SetQueryWithSearchResult();
            Category = SearchResult.Category;
            AdvancedSearch = (SearchResult as AdvancedSearchResult)?.AdvancedSearch ?? new AdvancedSearchOptions();
        }

        private Category _Category;

        public Category Category
        {
            get => _Category;
            set => Set(ref _Category, value);
        }

        private AdvancedSearchOptions _AdvancedSearch;
        public AdvancedSearchOptions AdvancedSearch
        {
            get => _AdvancedSearch;
            private set => Set(ref _AdvancedSearch, value);
        }
    }
}
