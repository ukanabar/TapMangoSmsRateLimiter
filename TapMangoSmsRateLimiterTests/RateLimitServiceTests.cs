using Microsoft.Extensions.Options;
using Moq;
using TapMangoSmsRateLimiter.Configurations;
using TapMangoSmsRateLimiter.Services.Kafka;
using TapMangoSmsRateLimiter.Services.RateLimit;
using TapMangoSmsRateLimiter.Services.Redis;

namespace TapMangoSmsRateLimiterTests
{
    public class RateLimitServiceTests
    {
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly Mock<IKafkaProducerService> _kafkaProducerServiceMock;
        private readonly RateLimitService _rateLimitService;
        private readonly RateLimitOptions _rateLimitOptions;
        private readonly KafkaOptions _kafkaOptions;

        public RateLimitServiceTests()
        {
            _redisServiceMock = new Mock<IRedisService>();
            _kafkaProducerServiceMock = new Mock<IKafkaProducerService>();
            _rateLimitOptions = new RateLimitOptions
            {
                Accounts = new Dictionary<int, AccountRateLimit>
                {
                    { 1, new AccountRateLimit { MessagesPerSecondPerNumber = 2, MessagesPerSecondAccountWide = 5 } }
                }
            };
            _kafkaOptions = new KafkaOptions { BootstrapServers = "localhost:9092", Topic = "sms_rate_limit" };
            _rateLimitService = new RateLimitService(_redisServiceMock.Object, Options.Create(_rateLimitOptions), _kafkaProducerServiceMock.Object, Options.Create(_kafkaOptions));
        }

        [Fact]
        public async Task CanSendMessageAsync_ValidInputs_ReturnsTrue()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 1234567890;

            _redisServiceMock.Setup(r => r.CanSendMessageAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<AccountRateLimit>(), It.IsAny<TimeSpan>())).ReturnsAsync(true);
            
            // Act
            var result = await _rateLimitService.CanSendMessageAsync(accountId, phoneNumber);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CanSendMessageAsync_ExceedsAccountLimit_ReturnsFalse()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 1234567890;

            _redisServiceMock.Setup(r => r.CanSendMessageAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<AccountRateLimit>(), It.IsAny<TimeSpan>())).ReturnsAsync(false);


            // Act
            var result = await _rateLimitService.CanSendMessageAsync(accountId, phoneNumber);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanSendMessageAsync_InvalidPhoneNumber_ThrowsArgumentException()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 12345; // Invalid phone number

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _rateLimitService.CanSendMessageAsync(accountId, phoneNumber));
        }

        [Fact]
        public async Task CanSendMessageAsync_InvalidAccountNumber_ThrowsArgumentException()
        {
            // Arrange
            int accountId = 999; // Non-existent account
            long phoneNumber = 1234567890;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _rateLimitService.CanSendMessageAsync(accountId, phoneNumber));
        }
    }
}
