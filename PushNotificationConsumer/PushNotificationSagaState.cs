using MassTransit;

namespace PushNotificationConsumer
{
    public class PushNotificationSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string[] RecipientDeviceIds { get; set; }
        public int RetryCount { get; set; }
    }
}