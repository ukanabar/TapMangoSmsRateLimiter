namespace RateLimitDataConsumerWorkerService.Services.Cassandra
{
    public interface ICassandraService
    {
        Task InsertRecordAsync(int accountId, long phoneNumber, bool canSend, DateTime dateTime);
    }
}
