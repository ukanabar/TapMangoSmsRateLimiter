namespace TapMangoSmsRateLimiter.Configurations
{
    public class AccountRateLimit
    {
        public int MessagesPerSecondPerNumber { get; set; }
        public int MessagesPerSecondAccountWide { get; set; }
    }
}
