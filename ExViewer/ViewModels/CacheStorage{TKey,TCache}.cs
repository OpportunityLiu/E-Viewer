using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExViewer.ViewModels
{
    class CacheStorage<TKey, TCache>
    {
        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader)
            : this(loader, 100) { }

        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader, int maxCount)
            : this(loader, maxCount, null) { }

        public CacheStorage(Func<TKey, IAsyncOperation<TCache>> loader, int maxCount, IEqualityComparer<TKey> comparer)
            : this(maxCount, comparer)
        {
            this.asyncLoader = loader;
        }

        public CacheStorage(Func<TKey, TCache> loader)
                : this(loader, 100, null) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount)
                : this(loader, maxCount, null) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount, IEqualityComparer<TKey> comparer)
                : this(maxCount, comparer)
        {
            this.loader = loader;
        }

        private CacheStorage(int maxCount, IEqualityComparer<TKey> comparer)
        {
            this.MaxCount = maxCount;
            this.cacheDictionary = new Dictionary<TKey, TCache>(maxCount, comparer);
            this.cacheQueue = new Queue<TKey>(maxCount);
        }

        private readonly Queue<TKey> cacheQueue;
        private readonly Dictionary<TKey, TCache> cacheDictionary;

        private readonly Func<TKey, IAsyncOperation<TCache>> asyncLoader;
        private readonly Func<TKey, TCache> loader;

        public int MaxCount
        {
            get; set;
        }

        public void Add(TKey key, TCache value)
        {
            EnsureCapacity();
            if (!this.cacheDictionary.ContainsKey(key))
                this.cacheQueue.Enqueue(key);
            this.cacheDictionary[key] = value;
        }

        public IAsyncOperation<TCache> GetAsync(TKey key)
        {
            EnsureCapacity();
            if (this.cacheDictionary.ContainsKey(key))
                return AsyncWrapper.CreateCompleted(this.cacheDictionary[key]);
            this.cacheQueue.Enqueue(key);
            if (this.asyncLoader == null)
            {
                var result = this.loader(key);
                this.cacheDictionary[key] = result;
                return AsyncWrapper.CreateCompleted(result);
            }
            return Run(async token =>
            {
                return this.cacheDictionary[key] = await this.asyncLoader(key);
            });
        }

        public TCache Get(TKey key)
        {
            EnsureCapacity();
            if (this.cacheDictionary.ContainsKey(key))
                return this.cacheDictionary[key];
            else
            {
                this.cacheQueue.Enqueue(key);
                if (this.asyncLoader != null)
                    return this.cacheDictionary[key] = this.asyncLoader(key).AsTask().Result;
                else
                    return this.cacheDictionary[key] = this.loader(key);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.cacheDictionary.ContainsKey(key);
        }

        public bool ContainsValue(TCache value)
        {
            return this.cacheDictionary.ContainsValue(value);
        }

        public bool TryGet(TKey key, out TCache value)
        {
            return this.cacheDictionary.TryGetValue(key, out value);
        }

        public void EnsureCapacity()
        {
            while (this.cacheQueue.Count > this.MaxCount)
                this.cacheDictionary.Remove(this.cacheQueue.Dequeue());
        }
    }
}
