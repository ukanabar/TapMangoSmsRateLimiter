using Cassandra;
using Microsoft.Extensions.Options;
using RateLimitDataConsumerWorkerService.Configurations;

namespace RateLimitDataConsumerWorkerService.Services.Cassandra
{
    public class CassandraService : ICassandraService
    {
        private readonly ISession _session;

        public CassandraService(IOptions<CassandraOptions> cassandraOptions)
        {
            var cluster = Cluster.Builder()
                .AddContactPoint(cassandraOptions.Value.ContactPoint)
                .Build();

            _session = cluster.Connect(cassandraOptions.Value.Keyspace);
        }

        public async Task InsertRecordAsync(int accountId, long phoneNumber, bool canSend, DateTime dateTime)
        {
            var query = "INSERT INTO sms_rate_limits (account_id, phone_number, can_send, datetime) VALUES (?, ?, ?, ?)";
            var statement = await _session.PrepareAsync(query);
            var boundStatement = statement.Bind(accountId, phoneNumber, canSend, dateTime);
            await _session.ExecuteAsync(boundStatement);
        }
    }
}
