using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Dobley.Workers.Notifications;

public class TelegramNotificationConsumerService(
    RabbitMqOptions rabbitMqOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<TelegramNotificationConsumerService> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(botToken))
        {
            logger.LogWarning("TELEGRAM_BOT_TOKEN is empty. Telegram notifications are disabled.");
            return Task.CompletedTask;
        }

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        channel.QueueDeclare(rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<TelegramNotificationMessage>(json);
                if (message == null)
                {
                    channel.BasicAck(args.DeliveryTag, multiple: false);
                    return;
                }

                await SendTelegramMessage(botToken, message, stoppingToken);
                channel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Telegram notification processing failed");
                channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(rabbitMqOptions.QueueName, autoAck: false, consumer);
        stoppingToken.Register(() =>
        {
            channel.Dispose();
            connection.Dispose();
        });

        return Task.CompletedTask;
    }

    private async Task SendTelegramMessage(string botToken, TelegramNotificationMessage message,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.PostAsJsonAsync(
            $"https://api.telegram.org/bot{botToken}/sendMessage",
            new
            {
                chat_id = message.ExternalId,
                text = message.Text
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
