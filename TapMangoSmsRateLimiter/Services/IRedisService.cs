namespace TapMangoSmsRateLimiter.Services
{
    public interface IRedisService
    {
        Task<long> GetKeyCountAsync(string key);
        Task<long> IncrementKeyAsync(string key);        
        Task SetKeyExpirationAsync(string key, TimeSpan expiration);
    }
}
