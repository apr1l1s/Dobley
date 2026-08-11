namespace Dobley.Endpoints.Api.Dto;

public record StorageNotificationSubscriptionRequest(IReadOnlyList<int>? StorageIds, int NotifyBeforeDays = 3);
