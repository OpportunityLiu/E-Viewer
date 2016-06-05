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
        {
            this.asyncLoader = loader;
            this.MaxCount = maxCount;
        }

        public CacheStorage(Func<TKey, TCache> loader)
            : this(loader, 100) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount)
        {
            this.loader = loader;
            this.MaxCount = maxCount;
        }

        private Queue<TKey> cacheQueue = new Queue<TKey>();
        private Dictionary<TKey, TCache> cacheDictionary = new Dictionary<TKey, TCache>();

        private Func<TKey, IAsyncOperation<TCache>> asyncLoader;
        private Func<TKey, TCache> loader;
        public int MaxCount
        {
            get; set;
        }

        public void Add(TKey key, TCache value)
        {
            EnsureCapacity();
            if(!cacheDictionary.ContainsKey(key))
                cacheQueue.Enqueue(key);
            cacheDictionary[key] = value;
        }

        public IAsyncOperation<TCache> GetAsync(TKey key)
        {
            return Run(async token =>
            {
                EnsureCapacity();
                if(cacheDictionary.ContainsKey(key))
                    return cacheDictionary[key];
                else
                {
                    cacheQueue.Enqueue(key);
                    if(asyncLoader != null)
                        return cacheDictionary[key] = await asyncLoader(key);
                    else
                        return cacheDictionary[key] = loader(key);
                }
            });
        }

        public TCache Get(TKey key)
        {
            EnsureCapacity();
            if(cacheDictionary.ContainsKey(key))
                return cacheDictionary[key];
            else
            {
                cacheQueue.Enqueue(key);
                if(asyncLoader != null)
                    return cacheDictionary[key] = asyncLoader(key).AsTask().Result;
                else
                    return cacheDictionary[key] = loader(key);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return cacheDictionary.ContainsKey(key);
        }

        public bool ContainsValue(TCache value)
        {
            return cacheDictionary.ContainsValue(value);
        }

        public bool TryGet(TKey key, out TCache value)
        {
            return cacheDictionary.TryGetValue(key, out value);
        }

        public void EnsureCapacity()
        {
            while(cacheQueue.Count > MaxCount)
                cacheDictionary.Remove(cacheQueue.Dequeue());
        }
    }
}
