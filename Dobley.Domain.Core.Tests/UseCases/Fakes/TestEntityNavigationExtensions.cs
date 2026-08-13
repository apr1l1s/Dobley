using System.Reflection;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Entities.Storages;

namespace Dobley.Domain.Core.Tests.UseCases.Fakes;

public static class TestEntityNavigationExtensions
{
    public static void SetRecipient(this StorageNotificationSubscription subscription,
        NotificationRecipient recipient)
    {
        SetProperty(subscription, nameof(StorageNotificationSubscription.DomainNotificationRecipient), recipient);
    }

    public static void SetStorage(this StorageNotificationSubscription subscription, Storage storage)
    {
        SetProperty(subscription, nameof(StorageNotificationSubscription.DomainStorage), storage);
    }

    private static void SetProperty<TObject, TValue>(TObject target, string propertyName, TValue value)
    {
        typeof(TObject)
            .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }
}
