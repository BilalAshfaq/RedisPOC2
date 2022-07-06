using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisPOC.DTOs
{
    public class MultiCacheRequest
    {
        public Dictionary<string, string> KeyValuePairs { get; set; }
    }
}
