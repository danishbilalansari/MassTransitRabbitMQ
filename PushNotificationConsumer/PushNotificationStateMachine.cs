using MassTransit;
using PushNotificationContracts;

namespace PushNotificationConsumer
{
    // State machine to handle push notification process, tracking states and events
    public class PushNotificationStateMachine : MassTransitStateMachine<PushNotificationSagaState>
    {
        public PushNotificationStateMachine()
        {
            // Mapping the instance state to the current state of the saga
            InstanceState(x => x.CurrentState);

            // Define the events with correlation by the message's CorrelationId
            Event(() => NotificationRequested, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationSent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => NotificationFailed, x => x.CorrelateById(context => context.Message.CorrelationId));

            // Initial state: when the notification is requested
            Initially(
                When(NotificationRequested)  // Triggered when the notification request event is received
                    .Then(context =>
                    {
                        // Populate saga state with data from the notification request message
                        context.Instance.Title = context.Data.Title;
                        context.Instance.Body = context.Data.Body;
                        context.Instance.RecipientDeviceIds = context.Data.RecipientDeviceIds;
                    })
                    // Transition to the 'SendingNotification' state
                    .TransitionTo(SendingNotification)
                    // Publish the SendPushNotification message to initiate sending the notification
                    .Publish(context => new SendPushNotification
                    {
                        CorrelationId = context.Instance.CorrelationId,
                        Title = context.Instance.Title,
                        Body = context.Instance.Body,
                        RecipientDeviceIds = context.Instance.RecipientDeviceIds
                    }));

            // State where the notification is being sent
            During(SendingNotification,
                // If notification sent successfully, log and transition to 'Completed' state
                When(NotificationSent)
                    .Then(context => Console.WriteLine("Notification sent successfully."))
                    .TransitionTo(Completed),

                // If notification fails, handle retries or mark as failed
                When(NotificationFailed)
                    .Then(context =>
                    {
                        // Increment retry count
                        context.Instance.RetryCount++;

                        // Retry sending the notification up to 3 times
                        if (context.Instance.RetryCount < 3)
                        {
                            Console.WriteLine($"Retrying notification {context.Instance.CorrelationId}...");
                            context.Publish(new SendPushNotification
                            {
                                CorrelationId = context.Instance.CorrelationId,
                                Title = context.Instance.Title,
                                Body = context.Instance.Body,
                                RecipientDeviceIds = context.Instance.RecipientDeviceIds
                            });
                        }
                        else
                        {
                            // After 3 failed attempts, mark the notification as failed
                            Console.WriteLine($"Notification {context.Instance.CorrelationId} failed after 3 retries.");
                            context.Instance.CurrentState = "Failed";
                        }
                    }));

            // Finalize the state machine when the saga is completed
            SetCompletedWhenFinalized();
        }

        // Defining states
        public State SendingNotification { get; private set; }  // State where the notification is being sent
        public State Completed { get; private set; }            // Final state when the process is completed

        // Defining events (inputs) for the state machine
        public Event<SendPushNotification> NotificationRequested { get; private set; } // Event triggered when notification is requested
        public Event<PushNotificationSent> NotificationSent { get; private set; }      // Event triggered when the notification is successfully sent
        public Event<PushNotificationFailed> NotificationFailed { get; private set; }  // Event triggered when the notification fails
    }
}
