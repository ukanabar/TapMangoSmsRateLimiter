using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TapMangoSmsRateLimiter.Configurations;

namespace TapMangoSmsRateLimiter.Services.Kafka
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _kafkaProducer;

        public KafkaProducerService(IOptions<KafkaOptions> kafkaOptions)
        {
            var producerConfig = new ProducerConfig { BootstrapServers = kafkaOptions.Value.BootstrapServers };
            _kafkaProducer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task SendMessageAsync(string topic, string message)
        {
            await _kafkaProducer.ProduceAsync(topic, new Message<Null, string> { Value = message });
        }
    }
}
