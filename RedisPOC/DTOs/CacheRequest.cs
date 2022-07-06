using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisPOC.DTOs
{
    public class CacheRequest
    {
        public string Key { get; set;  }
        public string Value { get; set; }
    }
}
