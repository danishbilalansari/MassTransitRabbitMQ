# MassTransitRabbitMQ

1. PushNotificationConsumer
This project handles the consumption of push notifications and manages their lifecycle using a saga state machine.
Program.cs:
Sets up and configures MassTransit with RabbitMQ as the transport. Registers the PushNotificationConsumer to consume notifications and PushNotificationStateMachine to handle the saga for managing the push notification process. Defines two RabbitMQ queues: One for the consumer listening for push notifications (app-dev-send-push-notification). Another for the saga state machine tracking the notification's state (app-dev-send-push-notification-saga). Configures dead-letter queues for handling errors and skipped messages.
PushNotificationConsumer.cs:
Implements a consumer (IConsumer<SendPushNotification>) that consumes SendPushNotification messages. Simulates sending push notifications and handles success and failure cases by publishing PushNotificationSent and PushNotificationFailed events, respectively.
PushNotificationSagaState.cs:
Defines the state object that will be used by the saga state machine to track the notification process. Contains properties such as CorrelationId, Title, Body, RecipientDeviceIds, RetryCount, and CurrentState.
PushNotificationStateMachine.cs:
Implements a saga state machine using MassTransit to manage the lifecycle of push notifications. Tracks the states: Initially: When a notification is requested, it stores the details (Title, Body, RecipientDeviceIds) in the saga state and transitions to SendingNotification state. SendingNotification: On notification success (NotificationSent), it completes the saga. On failure (NotificationFailed), it retries the notification up to 3 times before marking it as failed.

2. PushNotificationContracts
This project contains the contracts (message definitions) that are shared between the producer and consumer.
SendPushNotification.cs:
Defines the contract for the message that triggers the push notification. It includes: CorrelationId: Unique identifier for the notification. Title: Title of the push notification. Body: Body content of the push notification. RecipientDeviceIds: Array of device IDs to which the notification is to be sent.
PushNotificationSent.cs:
Defines the contract for the event that is published when a push notification is successfully sent. It includes: CorrelationId: Unique identifier to correlate with the original SendPushNotification message.
PushNotificationFailed.cs:
Defines the contract for the event that is published when sending a push notification fails. It includes: CorrelationId: Unique identifier for the failed notification. Reason: The reason or error message for the failure.

3. PushNotificationProducer
This project handles the production and publishing of push notifications to RabbitMQ.
Program.cs:
Configures MassTransit to use RabbitMQ as the transport. Defines the exchange for publishing SendPushNotification messages and sets it to a fanout exchange named app-dev-send-push-notifications. Registers a background worker (Worker) that is responsible for sending notifications.
Worker.cs:
Implements IHostedService, which is a background worker service that continuously publishes push notifications to RabbitMQ. Within the StartAsync method: It creates multiple SendPushNotification messages with a title, body, and list of recipient device IDs. Publishes these notifications via RabbitMQ using the outbox pattern to ensure reliable message delivery. Handles exceptions and publishes a PushNotificationFailed event if the notification fails.
High-Level Summary:
Producer: The PushNotificationProducer project is responsible for creating and publishing push notification requests (SendPushNotification) to RabbitMQ using a fanout exchange. The Worker class sends multiple notifications in a loop.
Consumer: The PushNotificationConsumer project listens for these push notifications and processes them using a PushNotificationConsumer. It also tracks the notification process through a saga (PushNotificationStateMachine) that handles retries and state transitions (from 'requested' to 'sent' or 'failed').
Contracts: The PushNotificationContracts project holds the message types (or contracts) that define the structure of messages exchanged between the producer and the consumer, ensuring a shared understanding of the data being communicated.
This solution demonstrates a clear division of concerns: message production, consumption, and state management, all integrated with MassTransit and RabbitMQ.
