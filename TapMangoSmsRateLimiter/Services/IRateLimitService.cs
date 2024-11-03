namespace TapMangoSmsRateLimiter.Services
{
    public interface IRateLimitService
    {
        Task<bool> CanSendMessageAsync(int accountId, long phoneNumber);
    }
}
