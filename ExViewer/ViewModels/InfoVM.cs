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
            this.RefreshStatus.Tag = this;
            this.RefreshTaggingStatistics.Tag = this;
            this.OpenGallery.Tag = this;
            this.SearchTag.Tag = this;
            this.ResetImageUsage.Tag = this;
        }

        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public AsyncCommand RefreshStatus { get; }
            = AsyncCommand.Create(
                sender => ((InfoVM)sender.Tag).Status.RefreshAsync(),
                sender => ((InfoVM)sender.Tag).Status != null);

        public AsyncCommand RefreshTaggingStatistics { get; }
            = AsyncCommand.Create(
                sender => ((InfoVM)sender.Tag).TaggingStatistics.RefreshAsync(),
                sender => ((InfoVM)sender.Tag).TaggingStatistics != null);

        public AsyncCommand ResetImageUsage { get; }
            = AsyncCommand.Create(
                sender => ((InfoVM)sender.Tag).Status.ResetImageUsageAsync(),
                sender => ((InfoVM)sender.Tag).Status != null);

        public Command<TaggingRecord> OpenGallery { get; } = Command.Create<TaggingRecord>((sender, tr) =>
        {
            RootControl.RootController.TrackAsyncAction(GalleryVM.GetVMAsync(tr.GalleryInfo).AsAsyncAction(), async (s, e) =>
            {
                await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), tr.GalleryInfo.ID);
            });
        }, (sender, tr) => tr.GalleryInfo.ID > 0);

        public Command<TaggingRecord> SearchTag { get; } = Command.Create<TaggingRecord>(async (sender, tr) =>
        {
            var vm = SearchVM.GetVM(tr.Tag.Search(Category.All, new AdvancedSearchOptions(skipMasterTags: true, searchLowPowerTags: true)));
            await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery.ToString());
        }, (sender, tr) => tr.Tag.Content != null);
    }
}
