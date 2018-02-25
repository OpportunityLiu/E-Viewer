using ExClient;
using ExClient.Search;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using System;

namespace ExViewer.ViewModels
{
    public sealed class FavoritesVM : SearchResultVM<FavoritesSearchResult>
    {
        private static AutoFillCacheStorage<string, FavoritesVM> Cache = AutoFillCacheStorage.Create((string query) =>
        {
            var search = default(FavoritesSearchResult);
            if (string.IsNullOrEmpty(query))
            {
                search = Client.Current.Favorites.Search(string.Empty);
            }
            else
            {
                var uri = new Uri(query);

                var handle = ExClient.Launch.UriLauncher.HandleAsync(uri);
                search = (FavoritesSearchResult)((ExClient.Launch.SearchLaunchResult)handle.GetResults()).Data;
            }
            var vm = new FavoritesVM(search);
            AddHistory(vm.Keyword);
            return vm;
        }, 10);

        public static FavoritesVM GetVM(string query) => Cache.GetOrCreateAsync(query ?? string.Empty).GetResults();

        public static FavoritesVM GetVM(FavoritesSearchResult searchResult)
        {
            var vm = new FavoritesVM(searchResult ?? throw new ArgumentNullException(nameof(searchResult)));
            var query = vm.SearchQuery;
            AddHistory(vm.Keyword);
            Cache[query] = vm;
            return vm;
        }

        private FavoritesVM(FavoritesSearchResult searchResult)
            : base(searchResult) { }

        public override Command<string> Search { get; } = Command.Create<string>(async (sender, queryText) =>
        {
            var that = (FavoritesVM)sender.Tag;
            var cat = that.category;
            var search = cat == null ? Client.Current.Favorites.Search(queryText) : cat.Search(queryText);
            var vm = GetVM(search);
            await RootControl.RootController.Navigator.NavigateAsync(typeof(FavoritesPage), vm.SearchQuery);
        });

        public override void SetQueryWithSearchResult()
        {
            base.SetQueryWithSearchResult();
            this.Category = this.SearchResult.Category;
        }

        private FavoriteCategory category;
        public FavoriteCategory Category
        {
            get => this.category;
            set => Set(ref this.category, value);
        }
    }
}
