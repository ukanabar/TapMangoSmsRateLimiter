using StackExchange.Redis;
namespace TapMangoSmsRateLimiter.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<long> GetKeyCountAsync(string key)
        {
            var db = _redis.GetDatabase();
            var count = await db.StringGetAsync(key);
            return count.HasValue ? (long)count : 0;
        }

        public async Task<long> IncrementKeyAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.StringIncrementAsync(key);
        }        

        public async Task SetKeyExpirationAsync(string key, TimeSpan expiration)
        {
            var db = _redis.GetDatabase();
            await db.KeyExpireAsync(key, expiration);
        }
    }

}
