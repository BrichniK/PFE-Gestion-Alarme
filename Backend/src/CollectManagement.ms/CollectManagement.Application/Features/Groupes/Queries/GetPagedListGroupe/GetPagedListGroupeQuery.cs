namespace CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;

public record GetPagedListGroupeQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListGroupeResponse>;
