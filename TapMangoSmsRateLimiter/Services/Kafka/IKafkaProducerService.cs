namespace TapMangoSmsRateLimiter.Services.Kafka
{
    public interface IKafkaProducerService
    {
        Task SendMessageAsync(string topic, string message);
    }
}
