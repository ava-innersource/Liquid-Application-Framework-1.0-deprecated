using Liquid.Runtime;
using Liquid.Runtime.Configuration.Base;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using System;
using System.Threading.Tasks;

namespace Liquid.OnAzure
{
    /// <summary>
    ///  Include support of AzureRedis, that processing data included on Configuration file.
    /// </summary>
    public class AzureRedis : LightCache
    {
        private AzureRedisConfiguration config;
        private RedisCache _redisClient = null;
        private DistributedCacheEntryOptions _options = null;
        /// <summary>
        /// Initialize support of Cache and read file config
        /// </summary>
        public override void Initialize()
        {
            config = LightConfigurator.Config<AzureRedisConfiguration>("AzureRedis");
            _redisClient = new RedisCache(new RedisCacheOptions()
            {
                Configuration = config.Configuration,
                InstanceName = config.InstanceName
            });

            _options = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromSeconds(config.SlidingExpirationSeconds),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(config.AbsoluteExpirationRelativeToNowSeconds)
            };
        }
        /// <summary>
        /// Get Key on the Azure Redis server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public override T Get<T>(string key)
        {
            var data = _redisClient.Get(key);
            return FromByteArray<T>(data);
        }
        /// <summary>
        /// Get Key Async on the Azure Redis server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public override async Task<T> GetAsync<T>(string key)
        {
            var data = await _redisClient.GetAsync(key);
            return FromByteArray<T>(data);
        }
        /// <summary>
        /// Refresh key get on the Azure Redis server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public override void Refresh(string key)
        {
            _redisClient.Refresh(key);
        }
        /// <summary>
        /// Refresh async key get on the Azure Redis server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public override async Task RefreshAsync(string key)
        {
            await _redisClient.RefreshAsync(key);
        }
        /// <summary>
        ///  Remove key on the Azure Redis server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        public override void Remove(string key)
        {
            _redisClient.Remove(key);
        }
        /// <summary>
        ///  Remove async key on the Azure Redis server cache
        /// </summary>
        /// <param name="key">Key of object</param>
        /// <returns>Task</returns>
        public override Task RemoveAsync(string key)
        {
            return _redisClient.RemoveAsync(key);
        }
        /// <summary>
        /// Set Key  and value on the Azure Redis server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>object</returns>
        public override void Set<T>(string key, T value)
        {
            _redisClient.Set(key, ToByteArray(value), _options);
        }
        /// <summary>
        /// Set Key and value Async on the Azure Redis server cache
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="key">Key of object</param>
        /// <returns>Task with object</returns>
        public override async Task SetAsync<T>(string key, T value)
        {
            await _redisClient.SetAsync(key, ToByteArray(value), _options);
        }

        /// <summary>
        /// Method to run Health Check for Azure Redis
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override LightHealth.HealthCheck HealthCheck(string serviceKey, string value)
        {
            try
            {
                var redis = _redisClient.GetAsync(serviceKey);
                return LightHealth.HealthCheck.Healthy;
            }
            catch
            {
                return LightHealth.HealthCheck.Unhealthy;
            }   
        }
    }
}
