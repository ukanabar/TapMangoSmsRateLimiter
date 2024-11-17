using StackExchange.Redis;
using TapMangoSmsRateLimiter.Configurations;

namespace TapMangoSmsRateLimiter.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<long> GetKeyCountAsync(string key) =>
            (await _db.StringGetAsync(key)).TryParse(out long count) ? count : 0;

        public Task<long> IncrementKeyAsync(string key) =>
            _db.StringIncrementAsync(key);

        public Task SetKeyExpirationAsync(string key, TimeSpan expiration) =>
            _db.KeyExpireAsync(key, expiration);

        public async Task<bool> CanSendMessageAsync(
            int accountId,
            long phoneNumber,
            AccountRateLimit accountLimits,
            TimeSpan expiration)
        {
            string accountKey = GenerateKey("account", accountId);
            string numberKey = GenerateKey("number", phoneNumber);

            var tasks = new[]
            {
                GetKeyCountAsync(accountKey),
                GetKeyCountAsync(numberKey)
            };
            var counts = await Task.WhenAll(tasks);
            long accountCount = counts[0];
            long numberCount = counts[1];

            if (accountCount >= accountLimits.MessagesPerSecondAccountWide ||
                numberCount >= accountLimits.MessagesPerSecondPerNumber)
            {
                return false;
            }

            await Task.WhenAll(
                IncrementAndSetExpirationIfNeededAsync(accountKey, expiration),
                IncrementAndSetExpirationIfNeededAsync(numberKey, expiration)
            );

            return true;
        }

        private static string GenerateKey(string type, object id) =>
            $"{type}:{id}:count";

        private async Task IncrementAndSetExpirationIfNeededAsync(string key, TimeSpan expiration)
        {
            long newCount = await IncrementKeyAsync(key);
            if (newCount == 1)
            {
                await SetKeyExpirationAsync(key, expiration);
            }
        }
    }
}
