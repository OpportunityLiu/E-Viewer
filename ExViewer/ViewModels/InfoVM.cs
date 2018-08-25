using ExClient;
using ExClient.Search;
using ExClient.Status;
using ExViewer.Database;
using ExViewer.Views;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse.Commands;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ExViewer.ViewModels
{
    internal class InfoVM : ViewModelBase
    {
        public UserStatus Status => Client.Current.UserStatus;

        public TaggingStatistics TaggingStatistics => Client.Current.TaggingStatistics;

        public ObservableList<HistoryRecord> History { get; } = new ObservableList<HistoryRecord>();

        public AsyncCommand RefreshStatus => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.RefreshAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null));

        public AsyncCommand RefreshTaggingStatistics => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                  sender => ((InfoVM)sender.Tag).TaggingStatistics.RefreshAsync(),
                  sender => ((InfoVM)sender.Tag).TaggingStatistics != null));

        public AsyncCommand RefreshHistory => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                  async sender => ((InfoVM)sender.Tag).History.Update(await HistoryDb.GetAsync())));

        public AsyncCommand ResetImageUsage => Commands.GetOrAdd(() =>
            AsyncCommand.Create(
                 sender => ((InfoVM)sender.Tag).Status.ResetImageUsageAsync(),
                 sender => ((InfoVM)sender.Tag).Status != null));

        public Command<TaggingRecord> OpenGallery => Commands.GetOrAdd(() =>
            Command<TaggingRecord>.Create(
                (sender, tr) => UriHandler.Handle(tr.GalleryInfo.Uri),
                (sender, tr) => tr.GalleryInfo.ID > 0));

        public Command<TaggingRecord> SearchTag => Commands.GetOrAdd(() =>
            Command<TaggingRecord>.Create(async (sender, tr) =>
            {
                var vm = SearchVM.GetVM(tr.Tag.Search(Category.All, new AdvancedSearchOptions(skipMasterTags: true, searchLowPowerTags: true)));
                await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
            }, (sender, tr) => tr.Tag.Content != null));

        public Command<HistoryRecord> OpenHistory => Commands.GetOrAdd(() =>
            Command<HistoryRecord>.Create(
                (sender, hr) => UriHandler.Handle(hr.Uri),
                (sender, hr) => hr?.Uri != null));

        public Command<HistoryRecord> DeleteHistory => Commands.GetOrAdd(() =>
            Command<HistoryRecord>.Create((sender, hr) =>
            {
                HistoryDb.Remove(hr.Id);
                ((InfoVM)sender.Tag).History.Remove(hr);
            }, (sender, hr) => hr != null && hr.Id > 0));

        public Command<HistoryRecord> DeleteHistoryWithSameTitle => Commands.GetOrAdd(() =>
            Command<HistoryRecord>.Create((sender, hr) =>
            {
                var title = hr.Title;
                HistoryDb.Remove(r => r.Title == title);
                ((InfoVM)sender.Tag).RefreshHistory.Execute();
            }, (sender, hr) => hr != null && hr.Id > 0));

        public Command<HistoryRecord> DeleteHistoryWithSameDate => Commands.GetOrAdd(() =>
            Command<HistoryRecord>.Create((sender, hr) =>
            {
                var tbegin = hr.Time.LocalDateTime.Date;
                var tend = tbegin.AddDays(1);
                var tsbegin = new DateTimeOffset(tbegin).ToUnixTimeMilliseconds();
                var tsend = new DateTimeOffset(tend).ToUnixTimeMilliseconds();

                HistoryDb.Remove(r => r.TimeStamp >= tsbegin && r.TimeStamp < tsend);
                ((InfoVM)sender.Tag).RefreshHistory.Execute();
            }, (sender, hr) => hr != null && hr.Id > 0));

        public AsyncCommand<HistoryRecord> ClearHistory => Commands.GetOrAdd(() =>
            AsyncCommand<HistoryRecord>.Create(async (sender, tr) =>
            {
                await HistoryDb.ClearAsync();
                ((InfoVM)sender.Tag).History.Clear();
            }));
    }
}