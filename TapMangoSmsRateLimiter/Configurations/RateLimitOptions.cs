namespace TapMangoSmsRateLimiter.Configurations
{
    public class RateLimitOptions
    {
        public Dictionary<int, AccountRateLimit> Accounts { get; set; }
    }
}
