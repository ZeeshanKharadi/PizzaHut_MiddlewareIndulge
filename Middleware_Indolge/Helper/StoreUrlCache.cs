using Microsoft.Extensions.Caching.Memory;

namespace Middleware_Indolge.Helper
{
    public static class StoreUrlCache
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public static string GetStoreUrl(string storeId)
        {
            if (!_cache.TryGetValue(storeId, out string storeUrl))
            {
                storeUrl= string.Empty;
                //storeUrl = fetchFromDb(storeId);

                //if (!string.IsNullOrWhiteSpace(storeUrl))
                //{
                //    _cache.Set(storeId, storeUrl, TimeSpan.FromHours(1)); // or sliding expiration
                //}
            }
            
            return storeUrl;
        }
        public static bool SetStoreUrl(string storeId, string storeUrl)
        {
            if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(storeUrl))
                return false;

            _cache.Set(storeId, storeUrl, TimeSpan.FromHours(1));

            // Verify if saved
            return _cache.TryGetValue(storeId, out string cachedUrl) && cachedUrl == storeUrl;
        }
        public static void InvalidateStore(string storeId)
        {
            _cache.Remove(storeId); // useful if URL changes
        }
    }

}
