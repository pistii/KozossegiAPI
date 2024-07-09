using Microsoft.Extensions.Caching.Memory;

namespace KozossegiAPI.Storage
{
    public class VerificationCodeCache : IVerificationCodeCache
    {
        /// <summary>
        /// Stores the verification codes 
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        public VerificationCodeCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Create(string verCode, string guid)
        {
            if (!_memoryCache.TryGetValue(verCode, out var val))
            {

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(900)); //15 perc után törlődik a gyorsítótárból

                _memoryCache.Set(verCode, guid, cacheEntryOptions);
                return;
            }

            _memoryCache.Set(verCode, guid);
            return;
        }

        public string GetValue(string verCode)
        {
            if (_memoryCache.TryGetValue(verCode, out string value))
            {
                return value;
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
        public void Remove(string verCode)
        {
            _memoryCache.Remove(verCode);
        }
    }

    public interface IVerificationCodeCache
    {
        /// <summary>
        /// Gets or creates the key value pairs by the hashed email and verification code
        /// </summary>
        /// <param name="hashed"></param>
        /// <param name="verCode"></param>
        void Create(string verCode, string guid);
        string GetValue(string verCode);
        void Remove(string verCode);
    }
}
