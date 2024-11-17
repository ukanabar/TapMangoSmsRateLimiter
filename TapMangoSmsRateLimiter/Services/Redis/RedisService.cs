using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TapMangoSmsRateLimiter.Configurations;
namespace TapMangoSmsRateLimiter.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;            
            _db = _redis.GetDatabase();
        }

        public async Task<long> GetKeyCountAsync(string key)
        {
            var count = await _db.StringGetAsync(key);
            return count.HasValue ? (long)count : 0;
        }

        public async Task<long> IncrementKeyAsync(string key)
        {
            return await _db.StringIncrementAsync(key);
        }

        public async Task SetKeyExpirationAsync(string key, TimeSpan expiration)
        {
            await _db.KeyExpireAsync(key, expiration);
        }

        public async Task<bool> CanSendMessageAsync(int accountId, long phoneNumber, AccountRateLimit accountLimits, TimeSpan expiration)
        {
            var accountKey = $"account:{accountId}:count";
            var numberKey = $"number:{phoneNumber}:count";

            var accountCount = await GetKeyCountAsync(accountKey);
            var numberCount = await GetKeyCountAsync(numberKey);

            // Check if limits are exceeded
            if (accountCount >= accountLimits.MessagesPerSecondAccountWide || numberCount >= accountLimits.MessagesPerSecondPerNumber)
            {
                return false;
            }

            var incrementedAccountCount = await IncrementKeyAsync(accountKey);

            if (incrementedAccountCount == 1)
            {
                await SetKeyExpirationAsync(accountKey, expiration);
            }

            var incrementedNumberCount = await IncrementKeyAsync(numberKey);

            if (incrementedNumberCount == 1)
            {
                await SetKeyExpirationAsync(numberKey, expiration);
            }

            return true;
        }
    }

}
