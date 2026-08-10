namespace Dobley.Domain.Core.Entities;

public interface IAuditableEntity
{
    DateTime DateAdded { get; }

    DateTime DateUpdated { get; }

    void SetDateAdded(DateTime dateAdded);

    void SetDateUpdated(DateTime dateUpdated);
}
