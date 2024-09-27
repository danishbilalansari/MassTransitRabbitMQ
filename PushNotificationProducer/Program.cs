using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PushNotificationContracts;
using System;

namespace PushNotificationProducer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create and configure the host application
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    // Configure MassTransit with RabbitMQ
                    services.AddMassTransit(x =>
                    {
                        // Configure RabbitMQ transport
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            // Set the RabbitMQ connection details
                            cfg.Host("rabbitmq://localhost", h =>
                            {
                                h.Username("guest");  // Default username for RabbitMQ
                                h.Password("guest");  // Default password for RabbitMQ
                            });

                            // Configure message topology for SendPushNotification message type
                            cfg.Message<SendPushNotification>(configTopology =>
                            {
                                // Set the exchange name using kebab-case naming convention
                                configTopology.SetEntityName("app-dev-send-push-notifications");
                            });

                            // Publish messages to the exchange of type fanout
                            cfg.Publish<SendPushNotification>(p =>
                            {
                                // Set exchange type to 'fanout' to broadcast messages to all bound queues
                                p.ExchangeType = "fanout";
                            });

                            // Configure all receive endpoints automatically based on consumers and sagas
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    // Register a hosted service (background worker) to produce messages
                    services.AddHostedService<Worker>();
                })
                .Build();

            // Run the host (application) asynchronously
            await host.RunAsync();
        }
    }
}