using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using TapMangoSmsRateLimiter.Configurations;

namespace TapMangoSmsRateLimiter.Services
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IRedisService _redisService;
        private readonly RateLimitOptions _rateLimitOptions;
        private readonly TimeSpan _rateLimitTimeout;

        public RateLimitService(IRedisService redisService, IOptions<RateLimitOptions> rateLimitOptions)
        {
            _redisService = redisService;
            _rateLimitOptions = rateLimitOptions.Value;
            _rateLimitTimeout = TimeSpan.FromSeconds(1);
        }

        public async Task<bool> CanSendMessageAsync(int accountId, long phoneNumber)
        {
            if (!_rateLimitOptions.Accounts.TryGetValue(accountId, out var accountLimits))
            {
                throw new ArgumentException("Account not found.");
            }

            if (phoneNumber.ToString().Length != 10 || !Regex.IsMatch(phoneNumber.ToString(), "^[0-9]+$"))
            {
                throw new ArgumentException("Invalid phone number format.");
            }

            // Keys for rate limiting
            var accountKey = $"account:{accountId}:count";
            var numberKey = $"number:{phoneNumber}:count";

            // Get current counts
            var accountCount = await _redisService.GetKeyCountAsync(accountKey);
            var numberCount = await _redisService.GetKeyCountAsync(numberKey);

            // Check if limits are exceeded
            if (accountCount >= accountLimits.MessagesPerSecondAccountWide || numberCount >= accountLimits.MessagesPerSecondPerNumber)
            {
                return false;
            }

            var incrementedAccountCount = await _redisService.IncrementKeyAsync(accountKey);

            if (incrementedAccountCount == 1)
            {
                await _redisService.SetKeyExpirationAsync(accountKey, _rateLimitTimeout);
            }

            var incrementedNumberCount = await _redisService.IncrementKeyAsync(numberKey);
            
            if (incrementedNumberCount == 1)
            {
                await _redisService.SetKeyExpirationAsync(numberKey, _rateLimitTimeout);
            }           

            return true;
        }
    }

}
