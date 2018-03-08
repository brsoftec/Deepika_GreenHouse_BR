using System;
using System.Runtime.Caching;

namespace GH.Core.BlueCode.BusinessLogic
{
    public class CacheContainer : IDisposable
    {
        private MemoryCache buffer;
        private object locker = new object();

        public CacheContainer(string cacheContainerName)
        {
            buffer = CacheManager.GetBuffer(cacheContainerName);
            if(buffer == null)
            {
                buffer = new MemoryCache(cacheContainerName);
                CacheManager.AddBuffer(cacheContainerName, buffer);
            }
        }

        public void Add(string key, object data,CachePolicy policy)
        {
            lock(locker)
            {
                var durationInSecond = policy.Duration * (int)policy.DurationType;
                DateTimeOffset timeOffset = DateTimeOffset.Now.AddSeconds(durationInSecond);
                buffer.Set(key, data, timeOffset);
            }
        }

        public object Get(string key)
        {
            return buffer.Get(key);
        }

        public T Get<T>(string key) 
        {
            return (T)buffer.Get(key);
        }

        public void Remove(string key)
        {
            lock(locker)
            {
                buffer.Remove(key);
            }
        }

        public void Dispose()
        {
            locker = null;
        }

    }
}
