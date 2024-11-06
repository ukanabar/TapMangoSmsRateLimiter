using Confluent.Kafka;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RateLimitDataConsumerWorkerService.Configurations;
using RateLimitDataConsumerWorkerService.Services.Cassandra;
using RateLimitDataConsumerWorkerService.Models;


namespace RateLimitDataConsumerWorkerService
{
    public class RateLimitDataConsumerService : BackgroundService
    {
        private readonly ILogger<RateLimitDataConsumerService> _logger;
        private readonly IConsumer<Null, string> _consumer;
        private readonly ICassandraService _cassandraService;
        private readonly string _topic;

        public RateLimitDataConsumerService(IOptions<KafkaOptions> kafkaOptions,
            ICassandraService cassandraService, ILogger<RateLimitDataConsumerService> logger)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaOptions.Value.BootstrapServers,
                GroupId = "cassandra-writer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Null, string>(config).Build();
            _topic = kafkaOptions.Value.Topic;
            _cassandraService = cassandraService;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);

                var record = JsonSerializer.Deserialize<MessageRecord>(consumeResult.Message.Value);

                if (record != null)
                {
                    await _cassandraService.InsertRecordAsync(record.AccountId, record.PhoneNumber, record.CanSend, record.DateTime);
                }
            }

            _consumer.Close();
        }
    }
}
