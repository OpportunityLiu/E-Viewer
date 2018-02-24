﻿using ExClient;
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
        where T : SearchResult
    {
        protected SearchResultVM(T searchResult)
        {
            this.searchResult = searchResult;
            this.Open.Tag = this;
            this.DeleteHistory.Tag = this;
            this.Search.Tag = this;
            SetQueryWithSearchResult();
        }

        public string SearchQuery => this.SearchResult.SearchUri.ToString();

        public Command<Gallery> Open { get; } = Command.Create<Gallery>(async (sender, g) =>
        {
            var that = (SearchResultVM<T>)sender.Tag;
            that.SelectedGallery = g;
            GalleryVM.GetVM(g);
            await RootControl.RootController.Navigator.NavigateAsync(typeof(GalleryPage), g.ID);
        }, (sender, g) => g != null);

        public abstract Command<string> Search { get; }

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

        public virtual void SetQueryWithSearchResult()
        {
            this.Keyword = this.SearchResult.Keyword;
        }

        private Gallery selectedGallery;
        public Gallery SelectedGallery { get => this.selectedGallery; protected set => Set(ref this.selectedGallery, value); }

        private string keyword;
        public string Keyword { get => this.keyword; set => Set(ref this.keyword, value); }

        private void SearchResult_LoadMoreItemsException(IncrementalLoadingList<Gallery> sender, LoadMoreItemsExceptionEventArgs args)
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

        internal Command<SearchHistory> DeleteHistory { get; } = Command.Create<SearchHistory>((sender, sh) =>
        {
            using (var db = new SearchHistoryDb())
            {
                db.SearchHistorySet.Remove(sh);
                db.SaveChanges();
            }
        }, (sender, sh) => sh != null);

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