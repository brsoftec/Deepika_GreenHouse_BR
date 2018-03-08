using System.Collections.Generic;
using System.Runtime.Caching;

namespace GH.Core.BlueCode.BusinessLogic
{
    public static class CacheManager
    {
        private static Dictionary<string, MemoryCache> cacheList;

        private static object locker = new object();

        static CacheManager()
        {
            cacheList = new Dictionary<string, MemoryCache>();
        }

        public static void AddBuffer(string key, MemoryCache buffer)
        {
            if(!cacheList.ContainsKey(key))
            {
                lock (locker)
                {
                    cacheList.Add(key, buffer);
                }
            }
        }

        public static MemoryCache GetBuffer(string key)
        {
            return cacheList.ContainsKey(key) ? cacheList[key] : null;
        }

        public static void RemoveBuffer(string key)
        {
            lock(locker)
            {
                if(cacheList.ContainsKey(key))
                {
                    cacheList.Remove(key);
                }

                var buffer = GetBuffer(key);
                if(buffer!= null)
                {
                    buffer.Dispose();
                    buffer = null;
                }
            }
        }

        public static CacheContainer CreateCacheContainer(string key)
        {
            return new CacheContainer(key);
        }

    }
}
