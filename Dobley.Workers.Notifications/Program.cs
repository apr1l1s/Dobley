using Dobley.Data.Core;
using Dobley.Workers.Notifications.ExpirationNotifications;
using Dobley.Workers.Notifications.Telegram;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddCoreServices()
    .AddNotificationIntegrations()
    .AddScoped<IExpirationNotificationPublishingService, ExpirationNotificationPublishingService>()
    .AddScoped<ITelegramBotCommandHandler, TelegramBotCommandHandler>()
    .AddHostedService<ExpirationNotificationPublisherService>()
    .AddHostedService<TelegramBotLinkingService>()
    .AddHostedService<TelegramNotificationConsumerService>();

builder.AddDobleyLogging("Dobley.Workers.Notifications");
builder.Build().Run();
