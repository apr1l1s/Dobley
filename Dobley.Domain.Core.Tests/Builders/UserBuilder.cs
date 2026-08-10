using Dobley.Domain.Core.Entities.Users;

namespace Dobley.Domain.Core.Tests.Builders;

public static class UserBuilder
{
    public static User Build(string login = "demo", string password = "password-hash")
        => User.Create(login, password);
}
