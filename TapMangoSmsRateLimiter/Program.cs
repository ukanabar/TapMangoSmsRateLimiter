using StackExchange.Redis;
using TapMangoSmsRateLimiter.Configurations;
using TapMangoSmsRateLimiter.Services.Kafka;
using TapMangoSmsRateLimiter.Services.RateLimit;
using TapMangoSmsRateLimiter.Services.Redis;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.Configure<RateLimitOptions>(config.GetSection("RateLimit"));
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetValue<string>("RedisConnectionString")));
builder.Services.AddSingleton<IRedisService,RedisService>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<IRateLimitService,RateLimitService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
