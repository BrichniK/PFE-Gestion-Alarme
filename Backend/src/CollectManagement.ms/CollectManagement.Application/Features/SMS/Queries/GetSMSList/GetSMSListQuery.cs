namespace CollectManagement.Application.Features.SMS.Queries.GetSMSList;

public record GetSMSListQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetSMSListResponse>;
