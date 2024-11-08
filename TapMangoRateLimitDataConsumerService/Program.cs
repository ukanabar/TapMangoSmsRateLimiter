using RateLimitDataConsumerWorkerService;
using RateLimitDataConsumerWorkerService.Configurations;
using RateLimitDataConsumerWorkerService.Services.Cassandra;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

// Configure settings from appsettings.json
builder.Services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
builder.Services.Configure<CassandraOptions>(configuration.GetSection("Cassandra"));

// Register services
builder.Services.AddSingleton<ICassandraService, CassandraService>();
builder.Services.AddHostedService<RateLimitDataConsumerService>(); // Register the Kafka consumer as a hosted background service

var app = builder.Build();
await app.RunAsync();
