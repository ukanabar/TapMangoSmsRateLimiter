using Microsoft.AspNetCore.Mvc;
using Moq;
using TapMangoSmsRateLimiter.Controllers;
using TapMangoSmsRateLimiter.Services;

namespace TapMangoSmsRateLimiterTests
{
    public class RateLimitControllerTests
    {
        private readonly Mock<IRateLimitService> _rateLimitServiceMock;
        private readonly SmsController _controller;

        public RateLimitControllerTests()
        {
            _rateLimitServiceMock = new Mock<IRateLimitService>();
            _controller = new SmsController(_rateLimitServiceMock.Object);
        }

        [Fact]
        public async Task CanSendMessage_ValidRequest_ReturnsOk()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 1234567890;

            _rateLimitServiceMock.Setup(r => r.CanSendMessageAsync(accountId, phoneNumber)).ReturnsAsync(true);

            // Act
            var result = await _controller.CanSendMessage(accountId, phoneNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Message can be sent.", okResult.Value);
        }

        [Fact]
        public async Task CanSendMessage_ExceedsRateLimit_Returns429()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 1234567890;

            _rateLimitServiceMock.Setup(r => r.CanSendMessageAsync(accountId, phoneNumber)).ReturnsAsync(false);

            // Act
            var result = await _controller.CanSendMessage(accountId, phoneNumber);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(429, statusCodeResult.StatusCode);
            Assert.Equal("Rate limit exceeded.", statusCodeResult.Value);
        }

        [Fact]
        public async Task CanSendMessage_InvalidPhoneNumber_ReturnsBadRequest()
        {
            // Arrange
            int accountId = 1;
            long phoneNumber = 12345; // Invalid phone number

            _rateLimitServiceMock.Setup(r => r.CanSendMessageAsync(accountId, phoneNumber)).ThrowsAsync(new ArgumentException("Invalid phone number format."));

            // Act
            var result = await _controller.CanSendMessage(accountId, phoneNumber);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid phone number format.", badRequestResult.Value);
        }

        [Fact]
        public async Task CanSendMessage_InvalidAccountNumber_ReturnsBadRequest()
        {
            // Arrange
            int accountId = 999; // Non-existent account
            long phoneNumber = 1234567890;

            _rateLimitServiceMock.Setup(r => r.CanSendMessageAsync(accountId, phoneNumber)).ThrowsAsync(new ArgumentException("Account not found."));

            // Act
            var result = await _controller.CanSendMessage(accountId, phoneNumber);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Account not found.", badRequestResult.Value);
        }
    }
}
