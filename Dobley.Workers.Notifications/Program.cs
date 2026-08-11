using Dobley.Data.Core;
using Dobley.Workers.Notifications;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddCoreServices()
    .AddHttpClient()
    .AddSingleton<RabbitMqOptions>()
    .AddHostedService<ExpirationNotificationPublisherService>()
    .AddHostedService<TelegramBotLinkingService>()
    .AddHostedService<TelegramNotificationConsumerService>();

builder.AddDobleyLogging("Dobley.Workers.Notifications");
builder.Build().Run();
