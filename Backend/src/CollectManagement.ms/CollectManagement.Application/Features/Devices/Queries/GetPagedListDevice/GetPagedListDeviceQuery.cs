namespace CollectManagement.Application.Features.Devices.Queries.GetPagedListDevice;

public record GetPagedListDeviceQuery(
    string? Search,
    string? Sort,
    string? Order,
    int Page,
    int Size
) : IRequest<GetPagedListDeviceResponse>;
