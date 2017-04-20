using ExClient;
using ExViewer.Database;
using ExViewer.Settings;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Linq;
using EhTagTranslatorClient;
using ExClient.Collections;

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
                if(this.searchResult != null)
                    this.searchResult.LoadMoreItemsException -= this.SearchResult_LoadMoreItemsException;
                Set(ref this.searchResult, value);
                if(this.searchResult != null)
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
            if(!RootControl.RootController.Available)
                return;
            RootControl.RootController.SendToast(args.Exception, typeof(SearchPage));
            args.Handled = true;
        }

        internal static void AddHistory(string content)
        {
            using(var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Add(SearchHistory.Create(content));
                db.SaveChanges();
            }
        }

        internal RelayCommand<SearchHistory> DeleteHistory
        {
            get;
        } = new RelayCommand<SearchHistory>(sh =>
        {
            using(var db = new SearchHistoryDb())
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
                using(var db = new SearchHistoryDb())
                {
                    var history = ((IEnumerable<SearchHistory>)db.SearchHistorySet
                                                                 .Where(sh => sh.Content.Contains(input))
                                                                 .OrderByDescending(sh => sh.Time))
                                        .Distinct()
                                        .Select(sh => sh.SetHighlight(input));
                    var quoteCount = input.Count(c => c == '"');
                    var lastword = default(string);
                    var previous = input;
                    if(quoteCount == 0)
                    {
                        lastword = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        previous = input.Substring(0, input.Length - lastword.Length);
                    }
                    else if(quoteCount % 2 == 0)
                    {
                        if(input[input.Length - 1] != '"')
                        {
                            var qp = input.LastIndexOf('"');
                            var sp = input.LastIndexOf(' ', input.Length - 1, input.Length - qp);
                            if(sp != -1)
                            {
                                lastword = input.Substring(sp + 1);
                                previous = input.Substring(0, input.Length - lastword.Length);
                            }
                            else
                            {
                                lastword = input.Substring(qp + 1);
                                previous = input.Substring(0, input.Length - lastword.Length) + " ";
                            }
                        }
                        else
                        {
                            lastword = null;
                        }
                    }
                    else
                    {
                        var qp = input.LastIndexOf('"');
                        lastword = input.Substring(qp + 1).Trim();
                        if(qp == 0)
                            previous = "";
                        else
                        {
                            previous = input.Substring(0, qp);
                            if(!char.IsWhiteSpace(input[qp - 1]))
                                previous = previous + " ";
                        }
                    }
                    var dictionary = Enumerable.Empty<ITagRecord>();
                    if(lastword != null)
                    {
                        dictionary = TagRecordFactory.GetTranslatedRecords(lastword)
                            .Concat<ITagRecord>(TagRecordFactory.GetRecords(lastword))
                            .Where(t => t != null)
                            .OrderByDescending(t => t.Score)
                            .Take(10)
                            .Distinct(tagComparer)
                            .Select(tag => tag.SetPrevious(previous));
                    }
                    try
                    {
                        return ((IEnumerable<object>)AutoCompletion.GetCompletions(input)).Concat(dictionary).Concat(history).ToList().AsReadOnly();
                    }
                    catch(InvalidOperationException)
                    {
                        //Collection changed
                        return null;
                    }
                }
            }).AsAsyncOperation();
        }

        internal bool AutoCompleteFinished(object selectedSuggestion)
        {
            if(selectedSuggestion is SearchHistory)
                return true;
            return false;
        }

        public IAsyncAction ClearHistoryAsync()
        {
            return Run(async token =>
            {
                using(var db = new SearchHistoryDb())
                {
                    db.SearchHistorySet.RemoveRange(db.SearchHistorySet);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}