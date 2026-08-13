using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Dobley.Data.Core.Integrations.RabbitMq;

public class RabbitMqNotificationMessageConsumer(RabbitMqOptions rabbitMqOptions,
    ILogger<RabbitMqNotificationMessageConsumer> logger)
    : INotificationMessageConsumer
{
    public async Task ConsumeAsync(Func<TelegramNotificationMessage, CancellationToken, Task> handleMessage,
        CancellationToken cancellationToken)
    {
        var factory = CreateConnectionFactory();

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(rabbitMqOptions.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, args) =>
        {
            try
            {
                var message = DeserializeMessage(args.Body.ToArray());
                if (message != null)
                {
                    await handleMessage(message, cancellationToken);
                }

                channel.BasicAck(args.DeliveryTag, multiple: false);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Notification queue message processing failed");
                channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(rabbitMqOptions.QueueName, autoAck: false, consumer);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private ConnectionFactory CreateConnectionFactory()
        => new()
        {
            HostName = rabbitMqOptions.HostName,
            Port = rabbitMqOptions.Port,
            UserName = rabbitMqOptions.UserName,
            Password = rabbitMqOptions.Password,
            DispatchConsumersAsync = true
        };

    private static TelegramNotificationMessage? DeserializeMessage(byte[] body)
        => JsonSerializer.Deserialize<TelegramNotificationMessage>(Encoding.UTF8.GetString(body));
}
