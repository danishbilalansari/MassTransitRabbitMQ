using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PushNotificationContracts;

namespace PushNotificationConsumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddMassTransit(x =>
                    {
                        // Register the saga state machine for managing push notifications.
                        // InMemoryRepository is used here for storing saga states, which is sufficient for testing 
                        // or small-scale applications. In a production environment, a persistent repository might be used.
                        x.AddSagaStateMachine<PushNotificationStateMachine, PushNotificationSagaState>()
                            .InMemoryRepository();

                        // Register the consumer that will handle push notification messages.
                        x.AddConsumer<PushNotificationConsumer>();

                        // Configure RabbitMQ as the transport.
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            // Connect to RabbitMQ using the localhost instance and default guest credentials.
                            cfg.Host("rabbitmq://localhost", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            // Define a receive endpoint for the consumer to listen to the push notification queue.
                            cfg.ReceiveEndpoint("app-dev-send-push-notification", e =>
                            {
                                // Configure the consumer to listen for messages in this queue.
                                e.ConfigureConsumer<PushNotificationConsumer>(context);

                                // Bind the queue to a fanout exchange.
                                // The fanout exchange type distributes messages to all bound queues without needing routing keys.
                                e.Bind("app-dev-send-push-notifications", x =>
                                {
                                    x.ExchangeType = "fanout";
                                });

                                // Bind a dead letter queue to handle error scenarios for manual intervention.
                                // If a message fails to be processed, it will be sent to this error queue.
                                e.BindDeadLetterQueue("app-dev-send-push-notification-error", "app-dev-send-push-notification");
                            });

                            // Define another receive endpoint for the saga state machine.
                            cfg.ReceiveEndpoint("app-dev-send-push-notification-saga", e =>
                            {
                                // Configure the saga state machine to listen for saga-related messages.
                                e.ConfigureSaga<PushNotificationSagaState>(context);

                                // Bind the saga queue to the same fanout exchange to ensure that saga messages are routed properly.
                                e.Bind("app-dev-send-push-notifications", x =>
                                {
                                    x.ExchangeType = "fanout";
                                });

                                // Bind a skipped queue to handle unconfigured consumers.
                                // If a message arrives for a consumer that isn't configured, it will be sent here for further action.
                                e.BindDeadLetterQueue("app-dev-send-push-notification-skipped", "app-dev-send-push-notification-saga");
                            });

                            // Automatically configure all registered consumers, sagas, and other endpoints.
                            cfg.ConfigureEndpoints(context);
                        });
                    });
                })
                .Build();

            // Start the host to run the MassTransit consumers and sagas.
            await host.RunAsync();
        }
    }
}