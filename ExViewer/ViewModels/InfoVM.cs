using ExClient;
using ExClient.Search;
using ExClient.Status;
using ExViewer.Views;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ExViewer.ViewModels
{
    public class InfoVM : ViewModelBase
    {
        public InfoVM()
        {
            Commands[nameof(RefreshStatus)] = AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.RefreshAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null);
            Commands[nameof(RefreshTaggingStatistics)] = AsyncCommand.Create(
                  sender => ((InfoVM)sender.Tag).TaggingStatistics.RefreshAsync(),
                  sender => ((InfoVM)sender.Tag).TaggingStatistics != null);
            Commands[nameof(ResetImageUsage)] = AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.ResetImageUsageAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null);
            Commands[nameof(OpenGallery)] = Command<TaggingRecord>.Create((sender, tr) =>
            {
                RootControl.RootController.TrackAsyncAction(GalleryVM.GetVMAsync(tr.GalleryInfo).ContinueWith(async _
                    => await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), tr.GalleryInfo.ID)));
            }, (sender, tr) => tr.GalleryInfo.ID > 0);
            Commands[nameof(SearchTag)] = Command<TaggingRecord>.Create(async (sender, tr) =>
            {
                var vm = SearchVM.GetVM(tr.Tag.Search(Category.All, new AdvancedSearchOptions(skipMasterTags: true, searchLowPowerTags: true)));
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }, (sender, tr) => tr.Tag.Content != null);
        }

        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public AsyncCommand RefreshStatus => GetCommand<AsyncCommand>();

        public AsyncCommand RefreshTaggingStatistics => GetCommand<AsyncCommand>();

        public AsyncCommand ResetImageUsage => GetCommand<AsyncCommand>();

        public Command<TaggingRecord> OpenGallery => GetCommand<Command<TaggingRecord>>();

        public Command<TaggingRecord> SearchTag => GetCommand<Command<TaggingRecord>>();
    }
}