using Dobley.Domain.Core.Entities.Notifications;

namespace Dobley.Data.Core.Integrations.RabbitMq;

public record NotificationMessage(Guid MessageId, NotificationChannel Channel, string Destination, string Subject,
    string Body);
