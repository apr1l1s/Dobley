namespace Dobley.Domain.Core.Entities.Users;

public class User
    : IAuditableEntity, ISoftDeletedEntity
{
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime DateAdded { get; private set; }

    public DateTime DateUpdated { get; private set; }

    public DateTime? DateDeleted { get; private set; }

    public bool IsDeleted => DateDeleted.HasValue;

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

    public void SetDateAdded(DateTime dateAdded) => DateAdded = dateAdded;

    public void SetDateUpdated(DateTime dateUpdated) => DateUpdated = dateUpdated;

    public void Delete(DateTime dateDeleted) => DateDeleted = dateDeleted;
}
