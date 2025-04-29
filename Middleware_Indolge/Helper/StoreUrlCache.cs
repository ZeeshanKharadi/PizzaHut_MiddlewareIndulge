using Microsoft.Extensions.Caching.Memory;

namespace Middleware_Indolge.Helper
{
    public static class StoreUrlCache
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static string GetStoreUrl(string storeId, Func<string, string> fetchFromDb)
        {
            if (!_cache.TryGetValue(storeId, out string storeUrl))
            {
                storeUrl = fetchFromDb(storeId);

                if (!string.IsNullOrWhiteSpace(storeUrl))
                {
                    _cache.Set(storeId, storeUrl, TimeSpan.FromHours(1)); // or sliding expiration
                }
            }

            return storeUrl;
        }

        public static void InvalidateStore(string storeId)
        {
            _cache.Remove(storeId); // useful if URL changes
        }
    }

}
