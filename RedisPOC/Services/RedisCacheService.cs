using RedisPOC.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisPOC.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<string[]> GetMultipleValuesAsync(List<string> keys)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var redisKeys = new List<RedisKey>();
            //var redisKeys = keys.Select(o => RedisKey(o));
            foreach (var key in keys)
            {
                var tempKey = new RedisKey(key);
                redisKeys.Add(tempKey);
            }
            var returnList = await db.StringGetAsync(keys: redisKeys.ToArray());
            return returnList.Select(o => o.ToString()).ToArray();
        }

        public async Task<string> GetValueAsync(string key)
        {
            var db = _connectionMultiplexer.GetDatabase();
            return await db.StringGetAsync(key);
        }
        
        public async Task SetValueAsync(string key, string value)
        {
            var db = _connectionMultiplexer.GetDatabase();
            await db.StringSetAsync(key, value);
        }
        public async Task SetMultipleValuesAsync(Dictionary<string, string> keyValuePairs)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var pairs = new List<KeyValuePair<RedisKey, RedisValue>>();
            foreach (var pair in keyValuePairs)
            {
                var tempKey = new KeyValuePair<RedisKey, RedisValue>(pair.Key, pair.Value);
                pairs.Add(tempKey);
            }
            await db.StringSetAsync(pairs.ToArray());
        }
    }
}
