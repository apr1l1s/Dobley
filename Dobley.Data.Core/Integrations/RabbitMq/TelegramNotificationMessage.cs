namespace Dobley.Data.Core.Integrations.RabbitMq;

public record TelegramNotificationMessage(string ExternalId, string Text);
