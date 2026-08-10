namespace Dobley.Domain.Core.Entities;

public interface ISoftDeletedEntity
{
    DateTime? DateDeleted { get; }

    bool IsDeleted { get; }

    void Delete(DateTime dateDeleted);
}
