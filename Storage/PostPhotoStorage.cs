using Microsoft.Extensions.Caching.Memory;

namespace KozossegiAPI.Storage
{
    public class PostPhotoStorage : IPostPhotoStorage
    {
        private readonly IMemoryCache _memoryCache;

        public PostPhotoStorage(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Create(string fileToken, byte[] byteArray)
        {
            if (!_memoryCache.TryGetValue(fileToken, out var bytes))
            {

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(900));

                _memoryCache.Set(fileToken, bytes, cacheEntryOptions);
                return;
            }

            _memoryCache.Set(fileToken, byteArray);
            return;
        }

        public byte[] GetValue(string fileToken)
        {
            if (_memoryCache.TryGetValue(fileToken, out byte[] byteArray))
            {
                return byteArray;
            }
            return null;
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-7.0
        /// Code should always have a fallback option to fetch data and not depend on a cached value being available.
        /// The cache uses a scarce resource, memory. Limit cache growth:
        /// Do not insert external input into the cache.As an example, using arbitrary user-provided input as a cache key is not recommended since the input might consume an unpredictable amount of memory.
        /// Use expirations to limit cache growth.
        /// Use SetSize, Size, and SizeLimit to limit cache size. The ASP.NET Core runtime does not limit cache size based on memory pressure. It's up to the developer to limit cache size.
        /// </summary>
        public void Remove(string fileToken)
        {
            _memoryCache.Remove(fileToken);
        }
    }

    public interface IPostPhotoStorage
    {
        void Create(string fileToken, byte[] byteArray);
        byte[] GetValue(string fileToken);
        void Remove(string fileToken);
    }
}
