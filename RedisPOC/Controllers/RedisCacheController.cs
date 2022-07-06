using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedisPOC.DTOs;
using RedisPOC.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisPOC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedisCacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public RedisCacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }


        [HttpGet("cache/{key}")]
        public async Task<IActionResult> GetValueAsync([FromRoute] string key)
        {
            var value = await _cacheService.GetValueAsync(key);
            return string.IsNullOrWhiteSpace(value) ? NotFound() : Ok(value);
        }

        [HttpGet("cache/keys")]
        public async Task<IActionResult> GetMultipleValuesAsync([FromQuery] List<string> keys)
        {
            var values = await _cacheService.GetMultipleValuesAsync(keys);
            return Ok(values);
        }

        [HttpPost("cache")]
        public async Task<IActionResult> SetValueAsync([FromBody] CacheRequest cacheRequest)
        {
            await _cacheService.SetValueAsync(cacheRequest.Key, cacheRequest.Value);
            return Ok();
        }

        [HttpPost("cache/add/multiple")]
        public async Task<IActionResult> SetMultipleValuesAsync([FromBody] MultiCacheRequest cacheRequest)
        {
            await _cacheService.SetMultipleValuesAsync(cacheRequest.KeyValuePairs);
            return Ok();
        }
    }
}
