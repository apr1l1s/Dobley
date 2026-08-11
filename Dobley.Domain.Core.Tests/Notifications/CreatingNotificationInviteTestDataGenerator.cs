namespace Dobley.Domain.Core.Tests.Notifications;

public record CreatingNotificationInviteTestCase(string TestName, string? UserName, string? Code, DateTime ExpiresAt,
    bool IsValid, string? ExpectedUserName = null, string? ExpectedCode = null)
{
    public override string ToString() => TestName;
}

public class CreatingNotificationInviteTestDataGenerator
    : DataGenerator<CreatingNotificationInviteTestCase>
{
    protected override IEnumerable<CreatingNotificationInviteTestCase> GetData()
    {
        yield return new CreatingNotificationInviteTestCase(
            TestName: "1.1 Корректный код подключения",
            UserName: "demo",
            Code: "ABC123",
            ExpiresAt: DateTime.UtcNow.AddDays(1),
            IsValid: true,
            ExpectedUserName: "demo",
            ExpectedCode: "ABC123");

        yield return new CreatingNotificationInviteTestCase(
            TestName: "1.2 Некорректный владелец кода подключения",
            UserName: null,
            Code: "ABC123",
            ExpiresAt: DateTime.UtcNow.AddDays(1),
            IsValid: false);

        yield return new CreatingNotificationInviteTestCase(
            TestName: "1.3 Некорректный код подключения",
            UserName: "demo",
            Code: null,
            ExpiresAt: DateTime.UtcNow.AddDays(1),
            IsValid: false);

        yield return new CreatingNotificationInviteTestCase(
            TestName: "1.4 Истёкший код подключения",
            UserName: "demo",
            Code: "ABC123",
            ExpiresAt: DateTime.UtcNow.AddDays(-1),
            IsValid: false);
    }
}
