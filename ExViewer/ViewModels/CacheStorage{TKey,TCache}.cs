using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.ViewModels
{
    class CacheStorage<TKey,TCache>
    {
        public CacheStorage(Func<TKey, TCache> loader)
            : this(loader, 100) { }

        public CacheStorage(Func<TKey, TCache> loader, int maxCount)
        {
            this.loader = loader;
            this.MaxCount = maxCount;
        }

        private Queue<TKey> cacheQueue = new Queue<TKey>();
        private Dictionary<TKey, TCache> cacheDictionary = new Dictionary<TKey, TCache>();

        private Func<TKey, TCache> loader;
        public int MaxCount
        {
            get; set;
        }

        public void Add(TKey key, TCache value)
        {
            EnsureCapacity();
            if(cacheQueue.Contains(key))
                cacheDictionary[key] = value;
            else
            {
                cacheQueue.Enqueue(key);
                cacheDictionary[key] = value;
            }
        }

        public TCache Get(TKey key)
        {
            EnsureCapacity();
            if(cacheQueue.Contains(key))
                return cacheDictionary[key];
            else
            {
                cacheQueue.Enqueue(key);
                return cacheDictionary[key] = loader(key);
            }
        }

        public void EnsureCapacity()
        {
            while(cacheQueue.Count > MaxCount)
                cacheDictionary.Remove(cacheQueue.Dequeue());
        }
    }
}
