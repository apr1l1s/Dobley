using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Dobley.Data.Core.Integrations.RabbitMq;

public class RabbitMqNotificationMessagePublisher(RabbitMqOptions rabbitMqOptions,
    ILogger<RabbitMqNotificationMessagePublisher> logger)
    : INotificationMessagePublisher
{
    public Task PublishAsync(TelegramNotificationMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: string.Empty, routingKey: rabbitMqOptions.QueueName, basicProperties: properties,
            body: body);

        logger.LogInformation("Published expiration notification for Telegram chat {ExternalId}", message.ExternalId);

        return Task.CompletedTask;
    }
}
