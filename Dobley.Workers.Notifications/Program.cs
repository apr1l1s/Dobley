using Dobley.Data.Core;
using Dobley.Workers.Notifications.ExpirationNotifications;
using Dobley.Workers.Notifications.Inbox;
using Dobley.Workers.Notifications.Options;
using Dobley.Workers.Notifications.Outbox;
using Dobley.Workers.Notifications.Senders;
using Dobley.Workers.Notifications.Telegram;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddCoreServices()
    .AddNotificationIntegrations()
    .AddNotificationWorkerOptions()
    .AddScoped<IExpirationNotificationPublishingService, ExpirationNotificationPublishingService>()
    .AddScoped<ITelegramBotCommandHandler, TelegramBotCommandHandler>()
    .AddSingleton<NotificationSenderRegistry>()
    .AddSingleton<INotificationSender, TelegramNotificationSender>()
    .AddHostedService<NotificationConfigurationValidatorService>()
    .AddHostedService<ExpirationNotificationPublisherService>()
    .AddHostedService<NotificationOutboxPublisherService>()
    .AddHostedService<TelegramBotPollingService>()
    .AddHostedService<NotificationInboxConsumerService>();

builder.AddDobleyLogging("Dobley.Workers.Notifications");
builder.Build().Run();
