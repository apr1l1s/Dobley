using User = Dobley.Domain.Core.Entities.Users.User;

namespace Dobley.Domain.Core.Tests.Users;

public class CreatingUserTests
{
    [Theory]
    [ClassData(typeof(CreatingUserTestDataGenerator))]
    public void Create_ReturnsUser(CreatingUserTestCase testCase)
    {
        var user = User.Create(testCase.Login, testCase.Password);

        Assert.Equal(testCase.ExpectedLogin, user.Login);
        Assert.Equal(testCase.ExpectedPassword, user.Password);
    }
}
