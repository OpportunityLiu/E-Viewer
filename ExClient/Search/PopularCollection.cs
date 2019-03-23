using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Search
{
    public sealed class PopularCollection : IncrementalLoadingList<Gallery>
    {
        private static readonly Uri _PopularUri = new Uri("/popular", UriKind.Relative);

        internal PopularCollection() { }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnPropertyChanged(nameof(HasMoreItems));
        }

        protected override void InsertItem(int index, Gallery item)
        {
            base.InsertItem(index, item);
            if (Count == 1)
            {
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            if (Count == 0)
            {
                OnPropertyChanged(nameof(HasMoreItems));
            }
        }

        public override bool HasMoreItems => Count == 0;

        private async Task<LoadItemsResult<Gallery>> _LoadCore(CancellationToken token)
        {
            var doc = await Client.Current.HttpClient.GetDocumentAsync(_PopularUri);
            var galleries = await GalleryListParser.Parse(doc, token);
            return LoadItemsResult.Create(0, galleries);
        }

        protected override IAsyncOperation<LoadItemsResult<Gallery>> LoadItemsAsync(int count)
            => AsyncInfo.Run(token => _LoadCore(token));
    }
}
