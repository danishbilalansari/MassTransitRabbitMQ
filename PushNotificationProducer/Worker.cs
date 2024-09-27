using MassTransit;
using Microsoft.Extensions.Hosting;
using PushNotificationContracts;

namespace PushNotificationProducer
{
    // The Worker class is a background service that produces push notifications.
    public class Worker : IHostedService
    {
        // IPublishEndpoint allows sending messages to RabbitMQ through MassTransit.
        private readonly IPublishEndpoint _publishEndpoint;

        // Constructor to inject IPublishEndpoint into the worker.
        public Worker(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        // This method is called when the background service is started.
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Simulate publishing 5 push notifications.
                for (int i = 0; i < 5; i++)
                {
                    // Create a new SendPushNotification message with a unique CorrelationId.
                    var notification = new SendPushNotification
                    {
                        CorrelationId = Guid.NewGuid(),  // Unique identifier for the notification.
                        Title = "Event Reminder",         // The title of the push notification.
                        Body = "Your scheduled event is in 30 minutes.",  // Notification message content.
                        RecipientDeviceIds = new[] { "device1", "device2", "device3" }  // List of recipient device IDs.
                    };

                    // Publish the notification message via the MassTransit outbox for reliable delivery.
                    await _publishEndpoint.Publish(notification);
                    Console.WriteLine($"Notification {notification.CorrelationId} published.");  // Log the success.
                }
            }
            catch (Exception ex)
            {
                // If an error occurs while publishing, handle it by publishing a PushNotificationFailed event.
                // This bypasses the outbox and sends the error notification immediately for urgent handling.
                await _publishEndpoint.Publish(new PushNotificationFailed { Reason = ex.Message });
                Console.WriteLine("Error publishing notification.");  // Log the error.
            }
        }

        // This method is called when the background service is stopped.
        // It is a no-op here as there's nothing to clean up, so it returns a completed task.
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
