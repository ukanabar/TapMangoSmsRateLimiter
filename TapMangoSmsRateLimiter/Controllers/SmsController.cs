using Microsoft.AspNetCore.Mvc;
using TapMangoSmsRateLimiter.Services.RateLimit;

namespace TapMangoSmsRateLimiter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;

        public SmsController(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        [HttpGet("can-send")] // Endpoint: api/rateLimit/can-send
        public async Task<IActionResult> CanSendMessage(int accountId, long phoneNumber)
        {
            try
            {
                var canSend = await _rateLimitService.CanSendMessageAsync(accountId, phoneNumber);
                if (!canSend)
                {
                    return StatusCode(429, "Rate limit exceeded.");
                }

                return Ok("Message can be sent.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}
