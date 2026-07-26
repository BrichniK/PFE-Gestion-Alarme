namespace CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;

public record GetPagedListSocieteQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
    ):IRequest<GetPagedListSocieteResponse>;