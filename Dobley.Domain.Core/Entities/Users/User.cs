namespace Dobley.Domain.Core.Entities.Users;

public class User
{
    public string Login { get; set; }

    public string Password { get; set; }

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