using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace TokenPriceUpdate.Providers
{
    public interface ICachedPriceProvider
    {
        Task<decimal> GetSwapTokenPriceAsync(string key);
        Task<decimal> GetExchangeTokenPriceAsync(string key);
        Task SetSwapTokenPriceAsync(string key, decimal price, DateTime dateTime);
        Task SetExchangeTokenPriceAsync(string key, decimal price, DateTime dateTime);
        Task<bool> IsCachedSwapPriceAsync(string key);
        Task ClearCacheAsync(string key);
    }

    public class CachedPriceProvider : ICachedPriceProvider, ISingletonDependency
    {
        private readonly IDistributedCache<CachedPrice> _swapTokenPriceCache;
        private readonly IDistributedCache<CachedPrice> _exchangePriceCache;

        public CachedPriceProvider(IDistributedCache<CachedPrice> swapTokenPriceCache,
            IDistributedCache<CachedPrice> exchangePriceCache)
        {
            _swapTokenPriceCache = swapTokenPriceCache;
            _exchangePriceCache = exchangePriceCache;
        }


        public async Task<decimal> GetSwapTokenPriceAsync(string key)
        {
            var cachedPrice = await _swapTokenPriceCache.GetAsync(key);
            return cachedPrice?.Price ?? -1m;
        }

        public async Task<decimal> GetExchangeTokenPriceAsync(string key)
        {
            var cachedPrice = await _exchangePriceCache.GetAsync(key);
            return cachedPrice?.Price ?? -1m;
        }

        public async Task SetSwapTokenPriceAsync(string key, decimal price, DateTime dateTime)
        {
            if (price < 0)
            {
                throw new Exception($"Invalid price: {price} for TokenKey: {key}");
            }

            var cachedInfo = await _swapTokenPriceCache.GetAsync(key);
            if (cachedInfo != null && cachedInfo.LatestUpdateDateTime >= dateTime)
            {
                return;
            }

            await _swapTokenPriceCache.SetAsync(key, new CachedPrice(price, dateTime));
        }

        public async Task SetExchangeTokenPriceAsync(string key, decimal price, DateTime dateTime)
        {
            if (price < 0)
            {
                throw new Exception($"Invalid price: {price} for TokenKey: {key}");
            }

            var cachedInfo = await _exchangePriceCache.GetAsync(key);
            if (cachedInfo != null && cachedInfo.LatestUpdateDateTime >= dateTime)
            {
                return;
            }

            await _exchangePriceCache.SetAsync(key, new CachedPrice(price, dateTime));
        }

        public async Task<bool> IsCachedSwapPriceAsync(string key)
        {
            return await _swapTokenPriceCache.GetAsync(key) != null;
        }

        public async Task ClearCacheAsync(string key)
        {
            await _swapTokenPriceCache.RemoveAsync(key);
            await _exchangePriceCache.RemoveAsync(key);
        }
    }

    public class CachedPrice
    {
        public CachedPrice(decimal price, DateTime dateTime)
        {
            Price = price;
            LatestUpdateDateTime = dateTime;
        }

        public CachedPrice()
        {
            
        }
        public decimal Price { get; set; }
        public DateTime LatestUpdateDateTime { get; set; }
    }
}