using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisPOC.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetValueAsync(string key);
        Task<string[]> GetMultipleValuesAsync(List<string> key);
        Task SetValueAsync(string key, string value);
        Task SetMultipleValuesAsync(Dictionary<string, string> keyValuePairs);
        void BenchMark();
    }
}
