namespace PushNotificationContracts
{
    public class SendPushNotification
    {
        public Guid CorrelationId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string[] RecipientDeviceIds { get; set; }
    }
}