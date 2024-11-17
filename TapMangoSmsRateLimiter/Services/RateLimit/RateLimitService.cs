using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.RegularExpressions;
using TapMangoSmsRateLimiter.Configurations;
using TapMangoSmsRateLimiter.Services.Kafka;
using TapMangoSmsRateLimiter.Services.Redis;

namespace TapMangoSmsRateLimiter.Services.RateLimit
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IRedisService _redisService;
        private readonly RateLimitOptions _rateLimitOptions;
        private readonly TimeSpan _rateLimitTimeout;
        private readonly IKafkaProducerService _kafkaProducerService;
        private readonly string _kafkaTopic;

        public RateLimitService(IRedisService redisService, IOptions<RateLimitOptions> rateLimitOptions, IKafkaProducerService kafkaProducerService, IOptions<KafkaOptions> kafkaOptions)
        {
            _redisService = redisService;
            _rateLimitOptions = rateLimitOptions.Value;
            _rateLimitTimeout = TimeSpan.FromMinutes(5);
            _kafkaProducerService = kafkaProducerService;
            _kafkaTopic = kafkaOptions.Value.Topic;
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

            bool canSend = await _redisService.CanSendMessageAsync(accountId, phoneNumber, accountLimits, _rateLimitTimeout);

            var record = new
            {
                AccountId = accountId,
                PhoneNumber = phoneNumber,
                DateTime = DateTime.UtcNow,
                CanSend = canSend
            };

            string message = JsonSerializer.Serialize(record);

            // Use the Kafka producer service to send the message
            await _kafkaProducerService.SendMessageAsync(_kafkaTopic, message);

            return canSend;

        }
    }

}
