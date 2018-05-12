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
        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public AsyncCommand RefreshStatus => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.RefreshAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null));

        public AsyncCommand RefreshTaggingStatistics => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                  sender => ((InfoVM)sender.Tag).TaggingStatistics.RefreshAsync(),
                  sender => ((InfoVM)sender.Tag).TaggingStatistics != null));

        public AsyncCommand ResetImageUsage => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.ResetImageUsageAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null));

        public Command<TaggingRecord> OpenGallery => Commands.GetOrAdd(() =>
            Command<TaggingRecord>.Create((sender, tr) =>
            {
                RootControl.RootController.TrackAsyncAction(GalleryVM.GetVMAsync(tr.GalleryInfo).ContinueWith(async _
                    => await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), tr.GalleryInfo.ID)));
            }, (sender, tr) => tr.GalleryInfo.ID > 0));

        public Command<TaggingRecord> SearchTag => Commands.GetOrAdd(() =>
            Command<TaggingRecord>.Create(async (sender, tr) =>
            {
                var vm = SearchVM.GetVM(tr.Tag.Search(Category.All, new AdvancedSearchOptions(skipMasterTags: true, searchLowPowerTags: true)));
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }, (sender, tr) => tr.Tag.Content != null));
    }
}