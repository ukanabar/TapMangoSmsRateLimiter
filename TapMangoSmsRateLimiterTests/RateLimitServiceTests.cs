using Microsoft.Extensions.Options;
using Moq;
using TapMangoSmsRateLimiter.Configurations;
using TapMangoSmsRateLimiter.Services;

namespace TapMangoSmsRateLimiterTests
{
    public class RateLimitServiceTests
    {
        private readonly Mock<IRedisService> _redisServiceMock;
        private readonly RateLimitService _rateLimitService;
        private readonly RateLimitOptions _rateLimitOptions;

        public RateLimitServiceTests()
        {
            _redisServiceMock = new Mock<IRedisService>();
            _rateLimitOptions = new RateLimitOptions
            {
                Accounts = new Dictionary<int, AccountRateLimit>
                {
                    { 1, new AccountRateLimit { MessagesPerSecondPerNumber = 2, MessagesPerSecondAccountWide = 5 } }
                }
            };
            _rateLimitService = new RateLimitService(_redisServiceMock.Object, Options.Create(_rateLimitOptions));
        }

        [Fact]
        public async Task CanSendMessageAsync_ValidInputs_ReturnsTrue()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 1234567890;

            _redisServiceMock.Setup(r => r.IncrementKeyAsync(It.IsAny<string>())).ReturnsAsync(1);
            _redisServiceMock.Setup(r => r.SetKeyExpirationAsync(It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(Task.CompletedTask);

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

            _redisServiceMock.Setup(r => r.GetKeyCountAsync(It.IsAny<string>())).ReturnsAsync(6);

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
