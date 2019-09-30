using Liquid.Runtime;
using System;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Liquid.Runtime.Configuration.Base;

namespace Liquid.OnWindowsClient
{
    /// <summary>
    ///  Include support of MemoryCache, that processing data included on Configuration file.
    /// </summary>
    public class MemoryCache : LightCache
    {
        /// <summary>
        /// Cache of memory
        /// </summary>
        private static System.Runtime.Caching.MemoryCache Cache = System.Runtime.Caching.MemoryCache.Default;

        private MemoryCacheConfiguration config;
        private CacheItemPolicy options = null;

        /// <summary>
        /// Initialize support of Cache and read file config
        /// </summary>
        public override void Initialize()
        {
            config = LightConfigurator.Config<MemoryCacheConfiguration>("MemoryCache");
            options = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromSeconds(config.SlidingExpirationSeconds),
                AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(config.AbsoluteExpirationRelativeToNowSeconds)
            };
        }
        /// <summary>
        /// Get Key on the MemoryCache server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public override T Get<T>(string key)
        {
            var data = (T)Cache.Get(key);
            return data;
        }
        /// <summary>
        /// Get Key Async on the MemoryCache server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public override async Task<T> GetAsync<T>(string key)
        {
            var data = (T)Cache.Get(key);
            return await Task.FromResult<T>(data);
        }
        /// <summary>
        /// Refresh key get on the MemoryCache server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public override void Refresh(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Refresh async key get on the MemoryCache server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public override async Task RefreshAsync(string key)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///  Remove key on the MemoryCache server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public override void Remove(string key)
        {
            Cache.Remove(key);
        }
        /// <summary>
        ///  Remove async key on the MemoryCache server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public override Task RemoveAsync(string key)
        {
            Cache.Remove(key);
            return Task.FromResult(true);
        }
        /// <summary>
        /// Set Key  and value on the MemoryCache server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public override void Set<T>(string key, T value)
        {
            Cache.Add(key, value, options);
        }
        /// <summary>
        /// Set Key and value Async on the MemoryCache server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public override async Task SetAsync<T>(string key, T value)
        {
            Cache.Add(key, value, options);
        }

        /// <summary>
        /// Method to run Health Check for MemoryCache
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var cache = Cache.Get(serviceKey);
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }
        }
    }
}
