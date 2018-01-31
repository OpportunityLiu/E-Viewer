using ExClient.Status;
using ExClient;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opportunity.MvvmUniverse.Commands;
using ExViewer.Views;
using ExClient.Search;
using Opportunity.Helpers.Universal.AsyncHelpers;

namespace ExViewer.ViewModels
{
    public class InfoVM : ViewModelBase
    {
        public InfoVM()
        {
            this.RefreshStatus = AsyncCommand.Create(() => Status.RefreshAsync().AsTask(), () => Status != null);
            this.RefreshTaggingStatistics = AsyncCommand.Create(() => TaggingStatistics.RefreshAsync().AsTask(), () => TaggingStatistics != null);
            this.OpenGallery = Command.Create<TaggingRecord>(tr =>
            {
                RootControl.RootController.TrackAsyncAction(GalleryVM.GetVMAsync(tr.GalleryInfo).AsAsyncAction(), (s, e) =>
                {
                    RootControl.RootController.Frame.Navigate(typeof(GalleryPage), tr.GalleryInfo.ID);
                });
            }, tr => tr.GalleryInfo.ID > 0);
            this.SearchTag = Command.Create<TaggingRecord>(tr =>
            {
                var vm = SearchVM.GetVM(tr.Tag.Search(Category.All, new AdvancedSearchOptions(skipMasterTags: true, searchLowPowerTags: true)));
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), vm.SearchQuery.ToString());
            }, tr => tr.Tag.Content != null);
            this.ResetImageUsage = AsyncCommand.Create(() => Status.ResetImageUsageAsync().AsTask(),
                () => Status != null);
        }

        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public AsyncCommand RefreshStatus { get; }

        public AsyncCommand RefreshTaggingStatistics { get; }

        public AsyncCommand ResetImageUsage { get; }

        public Command<TaggingRecord> OpenGallery { get; }

        public Command<TaggingRecord> SearchTag { get; }
    }
}
