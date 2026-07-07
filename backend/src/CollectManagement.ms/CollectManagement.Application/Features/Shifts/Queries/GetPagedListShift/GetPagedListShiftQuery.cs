namespace CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;

public record GetPagedListShiftQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListShiftResponse>;
