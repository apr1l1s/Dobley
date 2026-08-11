namespace Dobley.Workers.Notifications;

public class RabbitMqOptions
{
    public string HostName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "dobley.rabbitmq";

    public string UserName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "admin";

    public string Password { get; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "admin";

    public string QueueName { get; } = Environment.GetEnvironmentVariable("RABBITMQ_NOTIFICATION_QUEUE") ??
                                       "dobley.notifications";

    public int Port { get; } = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port)
        ? port
        : 5672;
}
