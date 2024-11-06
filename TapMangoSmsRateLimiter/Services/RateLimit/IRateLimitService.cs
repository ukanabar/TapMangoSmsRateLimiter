namespace TapMangoSmsRateLimiter.Services.RateLimit
{
    public interface IRateLimitService
    {
        Task<bool> CanSendMessageAsync(int accountId, long phoneNumber);
    }
}
