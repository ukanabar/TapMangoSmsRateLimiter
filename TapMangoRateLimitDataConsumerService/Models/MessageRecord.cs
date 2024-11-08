namespace RateLimitDataConsumerWorkerService.Models
{
    public class MessageRecord
    {
        public int AccountId { get; set; }
        public long PhoneNumber { get; set; }
        public bool CanSend { get; set; }
        public DateTime DateTime { get; set; }
    }
}
