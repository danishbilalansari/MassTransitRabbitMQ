using MassTransit;
using PushNotificationContracts;
using System;
using System.Threading.Tasks;

namespace PushNotificationConsumer
{
    public class PushNotificationConsumer : IConsumer<SendPushNotification>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        // Inject IPublishEndpoint
        public PushNotificationConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<SendPushNotification> context)
        {
            try
            {
                // Simulate consuming and processing the push notification
                Console.WriteLine($"Consuming and sending notification to: {string.Join(", ", context.Message.RecipientDeviceIds)}.");

                // Simulate a successful notification send
                Console.WriteLine("Notification sent successfully.");

                // Publish a success event using IPublishEndpoint (outside the context)
                await _publishEndpoint.Publish(new PushNotificationSent
                {
                    CorrelationId = context.Message.CorrelationId
                });
            }
            catch (Exception ex)
            {
                // Publish the failure event using IPublishEndpoint
                await _publishEndpoint.Publish(new PushNotificationFailed
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = ex.Message
                });
            }

            // Signal completion to indicate that the message has been consumed
            await Task.CompletedTask; // This confirms the message consumption
        }
    }
}