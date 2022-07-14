using RedisPOC.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            var sw = new Stopwatch();
            sw.Start();
            var returnList = await db.StringGetAsync(keys: redisKeys.ToArray());
            sw.Stop();
            var time = sw.Elapsed;
            return returnList.Select(o => o.ToString()).ToArray();
        }

        public async Task<string[]> GetMultipleValuesGroupAsync(List<string> keys)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var redisKeys = new List<RedisKey>();
            //var redisKeys = keys.Select(o => RedisKey(o));
            foreach (var key in keys)
            {
                var tempKey = new RedisKey(key);
                redisKeys.Add(tempKey);
            }
            var groupings = redisKeys.GroupBy(key => _connectionMultiplexer.GetHashSlot(key));
            var returnList = await db.StringGetAsync(keys: redisKeys.ToArray());
            return returnList.Select(o => o.ToString()).ToArray();
        }
        public async Task<string> GetValueAsync(string key)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var sw = new Stopwatch();
            sw.Start();
            var returnResult =  await db.StringGetAsync(key);
            sw.Stop();
            var time = sw.Elapsed;
            return returnResult;
        }
        
        public async Task SetValueAsync(string key, string value)
        {
            var db = _connectionMultiplexer.GetDatabase();
            var sw = new Stopwatch();
            sw.Start();
            await db.StringSetAsync(key, value);
            sw.Stop();
            var time = sw.Elapsed;
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
            var sw = new Stopwatch();
            sw.Start();
            await db.StringSetAsync(pairs.ToArray());
            sw.Stop();
            var time = sw.Elapsed;
        }

        public void BenchMark()
        {
            var db = _connectionMultiplexer.GetDatabase();

            RedisKey key = "devSmallKey";
            byte[] payload = new byte[3];
            new Random(12345).NextBytes(payload);
            RedisValue value = payload;

            Console.WriteLine($"key: {key.ToString()}");

            DoWork("PING (sync per op)", db, 1000, 5, x => { x.Ping(); return null; });
            DoWork("SET (sync per op)", db, 500, 5, x => { x.StringSet(key, value); return null; });
            DoWork("GET (sync per op)", db, 500, 5, x => { x.StringGet(key); return null; });

            DoWork("PING (pipelined per thread)", db, 1000, 5, x => x.PingAsync());
            DoWork("SET (pipelined per thread)", db, 500, 5, x => x.StringSetAsync(key, value));
            DoWork("GET (pipelined per thread)", db, 500, 5, x => x.StringGetAsync(key));
        }

        public void DoWork(string action, IDatabase db, int count, int threads, Func<IDatabase, Task> op)
        {
            object startup = new object(), shutdown = new object();
            int activeThreads = 0, outstandingOps = count;
            Stopwatch sw = default(Stopwatch);
            var threadStart = new ThreadStart(() =>
            {
                lock (startup)
                {
                    if (++activeThreads == threads)
                    {
                        sw = Stopwatch.StartNew();
                        Monitor.PulseAll(startup);
                    }
                    else
                    {
                        Monitor.Wait(startup);
                    }
                }
                Task final = null;
                while (Interlocked.Decrement(ref outstandingOps) >= 0)
                {
                    final = op(db);
                }
                if (final != null) final.Wait();
                lock (shutdown)
                {
                    if (--activeThreads == 0)
                    {
                        sw.Stop();
                        Monitor.PulseAll(shutdown);
                    }
                }
            });
            lock (shutdown)
            {
                for (int i = 0; i < threads; i++)
                {
                    new Thread(threadStart).Start();
                }
                Monitor.Wait(shutdown);
                Console.WriteLine($@"{action}
                {sw.ElapsedMilliseconds}ms for {count} ops on {threads} threads took {sw.Elapsed.TotalSeconds} seconds
                {(count * 1000) / sw.ElapsedMilliseconds} ops/s");
            }
        }
    }
}
