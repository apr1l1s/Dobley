namespace Dobley.Domain.Core.Repositories;

public record PaginatedCollection<TEntity>(IReadOnlyList<TEntity> Collection, int PageIndex = 1,
    int PageSize = 10, int TotalCount = 0);
