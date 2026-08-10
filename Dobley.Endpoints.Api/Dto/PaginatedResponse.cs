using Dobley.Domain.Core.Repositories;

namespace Dobley.Endpoints.Api.Dto;

public record PaginatedResponse<TItem>(IReadOnlyList<TItem> Collection, int PageIndex, int PageSize, int TotalCount)
{
    public static PaginatedResponse<TItem> Create<TDomainItem>(PaginatedCollection<TDomainItem> collection,
        Func<TDomainItem, TItem> mapItem)
        => new(collection.Collection.Select(mapItem).ToArray(), collection.PageIndex, collection.PageSize,
            collection.TotalCount);
}
