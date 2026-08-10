using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Errors.Entities;

namespace Dobley.Domain.Core.Entities.Storages;

public class Storage
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public User? DomainUser { get; set; }

    private Storage()
    {
    }

    public static Storage Create(string name, string description, User user)
    {
        var storage = Create(name, description, user.Login);
        storage.DomainUser = user;

        return storage;
    }

    public static Storage Create(string name, string description, string userName)
    {
        if (name.IsNullOrEmpty() || name.Length > 100)
        {
            throw new DomainValidateStorageException("Название хранилища должно быть не пустым и меньше 100 символов");
        }

        if (description.IsNullOrEmpty() || description.Length > 200)
        {
            throw new DomainValidateStorageException("Описание хранилища должно быть не пустым и меньше 200 символов");
        }

        if (userName.IsNullOrEmpty() || userName.Length > 100)
        {
            throw new DomainValidateStorageException("Логин владельца должен быть не пустой или меньше 100 символов");
        }

        return new Storage
        {
            Name = name,
            Description = description,
            UserName = userName
        };
    }

    public Storage Update(string? name, string? description, User? user)
    {
        if (name != null)
        {
            Name = name;
        }

        if (description != null)
        {
            Description = description;
        }

        if (user != null)
        {
            UserName = user.Login;
            DomainUser = user;
        }

        return this;
    }
}
