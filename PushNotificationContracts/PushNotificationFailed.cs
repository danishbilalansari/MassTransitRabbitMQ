namespace PushNotificationContracts
{
    public class PushNotificationFailed
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
    }
}
