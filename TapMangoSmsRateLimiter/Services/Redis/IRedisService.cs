using TapMangoSmsRateLimiter.Configurations;

namespace TapMangoSmsRateLimiter.Services.Redis
{
    public interface IRedisService
    {
        Task<long> GetKeyCountAsync(string key);
        Task<long> IncrementKeyAsync(string key);
        Task SetKeyExpirationAsync(string key, TimeSpan expiration);
        Task<bool> CanSendMessageAsync(int accountId, long phoneNumber, AccountRateLimit accountLimits, TimeSpan cacheExpiry);

    }
}
