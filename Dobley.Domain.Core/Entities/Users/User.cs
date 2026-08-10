namespace Dobley.Domain.Core.Entities.Users;

public class User
{
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    private User()
    {
    }

    public static User Create(string login, string password)
    {
        return new User()
        {
            Login = login,
            Password = password
        };
    }
}
