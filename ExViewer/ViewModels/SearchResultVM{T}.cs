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
using EhTagTranslatorClient;
using Opportunity.MvvmUniverse.Collections;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Commands;
using ExClient.Search;
using ExClient.Galleries;

namespace ExViewer.ViewModels
{
    public abstract class SearchResultVM<T> : ViewModelBase
        where T : SearchResultBase
    {
        private T searchResult;

        public T SearchResult
        {
            get => this.searchResult;
            protected set
            {
                if (this.searchResult != null)
                    this.searchResult.LoadMoreItemsException -= this.SearchResult_LoadMoreItemsException;
                Set(ref this.searchResult, value);
                if (this.searchResult != null)
                    this.searchResult.LoadMoreItemsException += this.SearchResult_LoadMoreItemsException;
            }
        }

        private Gallery selectedGallery;

        public Gallery SelectedGallery
        {
            get => this.selectedGallery;
            protected set => Set(ref this.selectedGallery, value);
        }

        private void SearchResult_LoadMoreItemsException(IncrementalLoadingCollection<Gallery> sender, LoadMoreItemsExceptionEventArgs args)
        {
            if (!RootControl.RootController.Available)
                return;
            RootControl.RootController.SendToast(args.Exception, typeof(SearchPage));
            args.Handled = true;
        }

        internal static void AddHistory(string content)
        {
            using (var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Add(SearchHistory.Create(content));
                db.SaveChanges();
            }
        }

        internal Command<SearchHistory> DeleteHistory
        {
            get;
        } = new Command<SearchHistory>(sh =>
        {
            using (var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Remove(sh);
                db.SaveChanges();
            }
        }, sh => sh != null);

        private class TagRecordEqulityComparer : IEqualityComparer<ITagRecord>
        {
            public bool Equals(ITagRecord x, ITagRecord y)
            {
                return x.TagToString() == y.TagToString();
            }

            public int GetHashCode(ITagRecord obj)
            {
                return (obj?.TagToString() ?? "").GetHashCode();
            }
        }

        private static readonly IEqualityComparer<ITagRecord> tagComparer = new TagRecordEqulityComparer();

        internal IAsyncOperation<IReadOnlyList<object>> LoadSuggestion(string input)
        {
            return Task.Run<IReadOnlyList<object>>(() =>
            {
                input = input?.Trim() ?? "";
                using (var db = new SearchHistoryDb())
                {
                    var history = ((IEnumerable<SearchHistory>)db.SearchHistorySet
                                                                 .Where(sh => sh.Content.Contains(input))
                                                                 .OrderByDescending(sh => sh.Time))
                                        .Distinct()
                                        .Select(sh => sh.SetHighlight(input));
                    AutoCompletion.SplitKeyword(input, out var lastwordNs, out var lastword, out var previous);
                    var dictionary = default(IEnumerable<ITagRecord>);
                    if (!string.IsNullOrEmpty(lastword))
                    {
                        dictionary = TagRecordFactory.GetTranslatedRecords(lastword, lastwordNs)
                            .Concat<ITagRecord>(TagRecordFactory.GetRecords(lastword, lastwordNs))
                            .Where(t => t != null)
                            .OrderByDescending(t => t.Score)
                            .Take(25)
                            .Distinct(tagComparer)
                            .Select(tag => tag.SetPrevious(previous));
                    }
                    else
                    {
                        dictionary = Enumerable.Empty<ITagRecord>();
                    }
                    try
                    {
                        return ((IEnumerable<object>)AutoCompletion.GetCompletions(input)).Concat(dictionary).Concat(history).ToList().AsReadOnly();
                    }
                    catch (InvalidOperationException)
                    {
                        //Collection changed
                        return null;
                    }
                }
            }).AsAsyncOperation();
        }

        internal bool AutoCompleteFinished(object selectedSuggestion)
        {
            if (selectedSuggestion is SearchHistory)
                return true;
            return false;
        }

        public IAsyncAction ClearHistoryAsync()
        {
            return Run(async token =>
            {
                using (var db = new SearchHistoryDb())
                {
                    db.SearchHistorySet.RemoveRange(db.SearchHistorySet);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}