namespace Dobley.Workers.Notifications.ExpirationNotifications;

public static class ExpirationNotificationMessageFactory
{
    public static string Create(string productName, string storageName, DateTime expirationDate, int daysLeft)
        => $"Братан, у продукта \"{productName}\" скоро закончится срок годности.\n"
           + $"Хранилище: {storageName}.\n"
           + $"Дата: {expirationDate:dd.MM.yyyy}.\n"
           + $"Осталось дней: {daysLeft}.";
}
