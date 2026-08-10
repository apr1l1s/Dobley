namespace Dobley.Domain.Core.Tests.Users;

public record CreatingUserTestCase(string TestName, string Login, string Password, string ExpectedLogin,
    string ExpectedPassword)
{
    public override string ToString() => TestName;
}

public class CreatingUserTestDataGenerator
    : DataGenerator<CreatingUserTestCase>
{
    protected override IEnumerable<CreatingUserTestCase> GetData()
    {
        yield return new CreatingUserTestCase(
            TestName: "1.1 Корректный пользователь с дефолтными значениями",
            Login: "demo",
            Password: "password-hash",
            ExpectedLogin: "demo",
            ExpectedPassword: "password-hash");

        yield return new CreatingUserTestCase(
            TestName: "1.2 Корректный пользователь с пользовательскими значениями",
            Login: "owner",
            Password: "custom-hash",
            ExpectedLogin: "owner",
            ExpectedPassword: "custom-hash");
    }
}
