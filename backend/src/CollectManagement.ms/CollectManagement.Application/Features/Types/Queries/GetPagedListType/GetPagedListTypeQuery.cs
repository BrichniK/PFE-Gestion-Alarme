namespace CollectManagement.Application.Features.Types.Queries.GetPagedListType;

public record GetPagedListTypeQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListTypeResponse>;
